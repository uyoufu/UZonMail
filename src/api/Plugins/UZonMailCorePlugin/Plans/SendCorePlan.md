# SendCore 发件内核一次性重构方案

发件内核位置：D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Services\SendCore

## 1. 重构目标与边界

本方案采用一次性替换，不保留新旧内核双跑或运行时切换开关。

- 保持现有 HTTP、Quartz、计划发送、立即发送、暂停、取消、重发、SignalR 参数和数据库状态枚举兼容。
- 修复职责链无条件执行、失败后循环清理、重复缓存、发件账户跨用户冲突、游离任务、递归重试以及显式代理失败后静默直连等问题。
- 内核以可扩展、可维护为首要约束，核心插件只定义稳定领域契约；Pro 插件通过扩展接口提供动态代理源和退订策略。
- 运行时仍为单实例内存实现，数据库继续作为业务状态来源；通过 `ISendLeaseStore` 隔离租约存储，为后续数据库租约和多实例部署保留替换点。
- 本次允许调整 SendCore 内部类型和接口，外部业务入口不变；不保留错误行为兼容。

## 2. 总体架构

固定执行链如下，每一层只有一个方向，不允许领域对象回调父容器：

```text
Command -> Activation -> Reader -> Scheduler -> Payload
        -> NetworkRoute -> Transport -> Outcome -> Commit -> Notify
```

| 层 | 核心职责 | 主要扩展点 |
| --- | --- | --- |
| Command | 接收立即发送、恢复、暂停、取消命令 | 保留 `ISendingGroupCommandService` |
| Activation | 恢复数据库状态并注册组、发件账户和 Reader | `ISendLeaseStore` |
| Reader | Keyset 分页读取轻量描述符，按需加载完整载荷 | `ISendItemPageSource`、`ISendPayloadReader` |
| Scheduler | 公平配额、发件账户串行、重试和租约分配 | `ISendingWorkerCoordinator`、`ISendLeaseStore` |
| NetworkRoute | 为每次 Transport 尝试解析直连、固定代理或动态代理 | `INetworkRouteResolver`、`IProxyFactory` |
| Transport | 统一 SMTP 和 Outlook Graph 发送与验证 | `IEmailTransport`、`SmtpConnector` |
| Outcome | 将异常或响应映射为稳定失败分类和后续动作 | `ITransportFailureClassifier` |
| Commit | 更新邮件、收件记录、组统计和发件账户状态 | 显式 Pipeline Stage |
| Notify | 数据库提交后发布 SignalR 通知和诊断快照 | `ISendRuntimeDiagnostics` |

核心规则：

- 运行时状态按职责封装：`SendingTasksManager` 只拥有工作任务，`UserGroupTasksPools` 只拥有组，`SenderAccountsManager` 只拥有发件账户，`SendItemReaderPool` 只拥有 Reader；这些类型均不再继承或暴露并发字典，锁内禁止数据库和异步调用。
- `SenderAccountKey` 固定为 `(UserId, SenderAccountId)`；Transport 会话键包含 `SenderAccountKey`、协议配置指纹和网络出口身份。
- 每封邮件在运行时只能处于 `Ready`、`Leased`、`Delayed`、`Terminal` 之一；成功、失败或取消后不再留在调度容器。
- `LeaseId` 用于拒绝暂停、取消或清理后迟到的结果；所有完成和清理命令必须幂等。
- 工作线程使用受限 `Channel` 接收唤醒，不进行空轮询；所有后台任务由协调器持有并观察异常。

## 3. Reader 与内存模型

### 3.1 两级分页

`SendItemReaderPool` 按发送组维护活动 Reader。Reader 不预加载全组 ID，而是使用稳定 Keyset 条件分页：

```text
WHERE SendingGroupId = @groupId
  AND Status IN (Created, Failed[, Pending])
  AND (SenderAccountId > @lastSenderAccountId OR (SenderAccountId = @lastSenderAccountId AND Id > @lastId))
ORDER BY SenderAccountId, Id
LIMIT @pageSize
```

恢复运行中的组时额外包含 `Pending`。描述符查询只选择 `Id`、`SendingGroupId`、`SenderAccountId`、`TriedCount`。完整 `SendingItem`、附件、模板和变量仅在取得执行槽后由 `ISendPayloadReader` 加载当前项；重试重新生成轻量条目，完成发送周期后立即释放完整载荷引用。

### 3.2 容量与回压

- 技术配置使用独立 `SendItemReaderOptions`，不得复用业务设置 `SendingSetting.MaxSendingBatchSize`。
- 默认描述符页大小 100；单组最多缓存 200；全局最多缓存 50,000；最多 256 个活动 Reader；完整载荷始终逐项水化。
- Reader 页缓存到达任一上限时停止继续读库；描述符转入组内单条目表后释放 Reader 容量，组内在就绪项低于预取水位时才继续取页。
- Reader 只保存游标、少量描述符和完成状态，不保存完整实体或 DbContext。
- 指定发件账户描述符只进入对应发件账户队列；共享描述符进入组共享 FIFO。指定队列优先于共享队列。
- 增加复合索引 `(SendingGroupId, Status, SenderAccountId, Id)`，同时生成 SQLite 和 PostgreSQL 迁移；旧单列索引可由迁移替换。

## 4. 调度、租约与多租户配额

新增 `SendingQuotaOptions`，业务默认值如下：

| 维度 | 默认并发 |
| --- | ---: |
| 系统硬上限 | 64 |
| 组织公平份额 | 16 |
| 用户公平份额 | 4 |
| 发送组公平份额 | 2 |
| 单发件账户会话 | 1 |

- 采用 work-conserving 调度：系统仍有空闲容量时，活跃组织、用户或组可以借用未使用份额。
- 借用不会抢占已开始发送；后续空闲槽优先分配给低于公平份额的租户，避免大租户长期占满。
- `ISendLeaseStore` 提供申请、完成、撤销、过期回收接口；本次实现 `InMemorySendLeaseStore`，进程启动时恢复数据库中 `Sending/Pending` 组。
- 系统、组织、用户和组配额由协调器按公平份额选择工作项；单项租约由 `ISendLeaseStore` 原子申请，同一发件账户通过运行标记保证同时最多一个活动发送。
- 组内保持加入顺序；延迟重试保存轻量 ID、发件账户和次数，并由可取消、可观察的到期任务重新入队，不占用发送工作槽。

## 5. 网络出口与代理架构

### 5.1 统一路由

`INetworkRouteResolver.ResolveAsync` 返回当前 Transport 尝试使用的 `NetworkRoute`：

- `DirectRoute`：未配置代理时使用。
- `StaticProxyRoute`：固定代理地址，按优先级、健康度、域名限速和使用次数选择。
- `DynamicProxyRoute`：由动态供应商按需拉取端点，带有效期和供应商身份。

显式配置 `ProxyId` 时，如果代理不存在、禁用、无法解析、供应商不可用或没有健康端点，本次发送必须返回代理错误，禁止静默直连。Outlook Graph 当前只支持直连，Graph 发件账户配置 `ProxyId` 属于验证错误。

### 5.2 固定与动态代理

- 保留基于 URL 自动识别动态供应商，不向数据库增加 `ProxyType` 字段。
- 每个 `IProxyFactory.CanHandle(Uri)` 必须按准确 host/path 判断；创建路由时必须恰好命中一个工厂，零命中或多命中均返回配置错误。
- 动态供应商负责获取和解析端点，不在客户端内部递归重试；重试、轮换和熔断统一由路由层控制。
- `Proxy.Priority` 进入候选排序；健康状态、租约计数、域名速率和冷却状态由路由管理器维护。
- 每次 Transport 重试均重新解析 `NetworkRoute`，SMTP 会话键包含路由身份，从而允许动态代理轮换且不会跨出口复用连接。
- 修正供应商工厂映射，杜绝一个供应商工厂返回另一个供应商客户端的情况。

## 6. SMTP 与 Outlook Graph 统一 Transport

### 6.1 抽象

```csharp
public interface IEmailTransport
{
    SendingProtocol Type { get; }
    Task<TransportResult> SendAsync(SendingContext context, MimeMessage message, CancellationToken token);
    Task<TransportResult> ValidateAsync(IServiceProvider provider, SenderEmailAddress senderAccount, CancellationToken token);
}
```

- SMTP 的连接、TLS 和认证复用 `SmtpConnector`；配置指纹和网络出口身份共同形成会话键。Graph 在自身 Transport 内统一解析认证配置并拒绝代理。
- `EmailSendersManager` 按 `SendingProtocol` 精确选择一个 Transport；缺失或重复实现均为配置错误。
- SMTP 和 Graph 都返回统一 `TransportResult`，包含分类、协议状态、错误码、服务端消息和回执 ID，不直接修改邮件或发件账户状态。
- SMTP 会话池按 `(SenderAccountKey, ProfileFingerprint, RouteIdentity)` 缓存，配置或出口变化自动失效；同一会话串行使用。
- Graph 会话按 `(SenderAccountKey, ProfileFingerprint)` 隔离，不再按邮箱字符串共享可变认证状态。
- Graph MIME 请求改为流式构造，避免 `MemoryStream -> ToArray -> Base64 string` 的多份大对象拷贝。

### 6.2 发件账户验证复用

- `SenderAccountValidateService` 与真实发送通过同一个 `EmailSendersManager -> IEmailTransport` 选择路径；SMTP 共同复用 `NetworkRouteResolver -> SmtpConnector`，Graph 共同复用客户端工厂与认证流程。
- SMTP 验证复用相同连接、TLS 和认证逻辑；Graph 验证复用相同 token 获取和 API 客户端构造逻辑。
- Graph 刷新凭据时按 `SenderAccountId` 持久化，禁止按 `(UserId, Email)` 模糊更新。
- 验证服务是写入 `IsValid`、`Status`、`ValidFailReason` 的唯一入口之一；Transport 本身不访问业务状态。

## 7. 失败、重试与后处理

| 分类 | 识别条件 | 处理结果 |
| --- | --- | --- |
| 收件人永久错误 | SMTP `RecipientNotAccepted` 5xx | 当前邮件失败，发件账户继续工作 |
| 邮件永久错误 | `MessageNotAccepted` 5xx、内容或策略拒绝 | 当前邮件失败，发件账户继续工作 |
| 发件账户永久错误 | `SenderNotAccepted` 5xx；432、530、534、535、538；认证、TLS、确定配置错误 | 持久禁用发件账户并退出运行池 |
| SMTP 瞬时错误 | 421、450、451、452、454 或明确 4xx | 短重试后进入队列退避 |
| 网络或代理错误 | Socket、IO、超时、连接中断、代理端点错误 | 重建会话并轮换代理，之后队列退避 |
| 本地数据错误 | 缺收件人、正文、模板、发送器或渲染失败 | 当前邮件失败，不影响发件账户 |
| 取消 | 主机停止、组暂停或取消令牌 | 撤销租约，不增加失败或重试次数 |
| 未知错误 | 无法可靠映射 | 重试耗尽后邮件失败，发件账户仅本次运行隔离 |

- 每个发送周期最多 3 次 Transport 尝试，即首次加 2 次短重试；不得使用递归。
- 队列级最大重试使用 `SendingSetting.MaxRetryCount`；`0` 表示不重试，缺省值为 3。
- 队列退避为 `2^retry` 秒加 0-25% 抖动，上限 60 秒；统一使用 `TimeProvider` 便于测试。
- 永久发件账户失败时先由 `SenderAccountEmailAddress` 原子标记为不可调度，再由 `PermanentSenderAccountFailureHandler` 调用 `SenderAccountRetirementCoordinator`，依次持久化失效状态、移出运行池、清理指定待发项并生成当前邮件决策；额度耗尽等正常退出则由后置 `SenderAccountRetirementHandler` 触发相同协调器。
- 当前共享邮件在还有其他共享发件账户且未耗尽重试时重新入队；指定到失效发件账户的邮件直接失败。
- 组失去全部可用发件账户时，剩余邮件失败并将组置为 `Pause`；瞬时或未知错误耗尽只隔离本次运行，不持久禁用。
- 显式 Pipeline 固定执行 `取项 -> 发送 -> 发件账户失效协调 -> 邮件提交/延迟重试 -> 组统计 -> 额度与会话收尾`。运行时条目只允许由 `GroupTask.CompleteEmailItem/ScheduleRetryEmailItem` 转移，数据库写入完成后再转移运行时状态和发送通知。

## 8. 核心与插件边界

- CorePlugin 定义 `IEmailTransport`、`INetworkRouteResolver`、`IProxyFactory`、`ISendingItemFilter`、Reader、租约和诊断契约。
- CorePlugin 实现 SMTP、Graph、固定代理、直接路由、调度、租约、提交和默认策略。
- ProPlugin 只实现动态代理供应商和退订策略，不直接操作 Core 运行时容器或数据库状态机。
- 邮件准备继续通过 `ISendItemPreparer` 隔离模板、变量和装饰器，Reader 与 Transport 不直接依赖渲染实现。
- Pro 资源接口改为依赖 `ISendRuntimeDiagnostics.GetSnapshot()`，保持现有 JSON 字段和含义。
- 删除递归 `SetNext` 职责链、反向 `Parent` 回调、回收站、未观察的 `Task.Run` 和不再使用的 Event/Reactive 代码。由 `SendingPipeline` 固定排序并按结果显式停止；`SendItemQueue` 仅保存轻量描述符，并以共享/指定 FIFO 索引调度。

## 9. 实施结果

本轮已完成一次性切换，未保留新旧内核双跑：

- 已落地分页 Reader、按需 Payload Reader、单条目运行表、内存租约、配额协调器、启动恢复和运行时诊断。
- 已落地统一网络路由，固定/动态代理均通过 `IProxyFactory` 精确匹配；显式代理不可用时返回失败，Graph 明确拒绝代理。
- 已落地 `IEmailTransport`，SMTP/Graph 的发送与验证复用各自连接或认证路径；SMTP 和 Graph 会话均使用发件账户与配置指纹隔离。
- 已将递归职责链改为显式顺序 Pipeline，移除父级回调、回收站、Event/Reactive 启动链和游离 `Task.Run`；失败清理、提交、组更新与会话收尾只沿 Pipeline 向后执行。
- 已生成 SQLite/PostgreSQL 复合索引迁移，并补充 Reader、租约、代理解析、SMTP 失败分类和条目状态测试。

原计划实施顺序已经完成：

1. 建立现有外部契约和失败分类的刻画测试，并修正 `MaxRetryCount=0` 语义。
2. 引入领域结果、Reader、租约存储、配额调度和只读运行时诊断，先做到独立可测试。
3. 实现统一网络出口、固定/动态代理源和严格代理失败语义。
4. 实现统一 Transport、SMTP/Graph 会话、验证复用和 Graph 凭据按 SenderAccountId 更新。
5. 实现 Payload、Outcome、Commit、Notify 周期及发件账户失效事务协调。
6. 将命令入口、Quartz、预览、装饰器和 Pro 资源接口切换到新内核，一次性移除旧执行链。
7. 生成 SQLite/PostgreSQL 复合索引迁移，格式化、运行测试并构建 CorePlugin、ProPlugin 和主服务。

每一步必须保持可编译；运行时切换只在新链完整后发生，不引入双跑。

## 10. 测试与验收

- Reader：验证 Keyset 不漏不重、分页边界、全局/单组回压、载荷按槽加载和释放。
- 租约：验证并发唯一性、过期结果拒绝、暂停/取消、启动恢复和幂等完成。
- 调度：验证系统硬上限、组织/用户/组公平份额、空闲借用、无抢占和发件账户单会话。
- 代理：验证优先级、准确供应商匹配、零/多匹配错误、动态刷新、健康冷却、轮换及禁止静默直连。
- Transport：数据驱动覆盖 SMTP 状态矩阵、Graph 错误、认证/TLS、网络、取消和未知异常；验证发送与发件账户校验走相同会话路径。
- 后处理：当前由状态容器与失败分类单元测试覆盖；成功、永久发件账户失败、共享改派、指定清理、无可用发件账户暂停和组完成仍应作为后续 SQLite 集成回归集持续补强。
- 契约：验证 HTTP、Quartz、SignalR、计划发送、重发、暂停、取消和 Pro 资源 JSON 不变。
- 性能：大组读取时每个工作槽只常驻当前完整实体，描述符数量受页大小、预取水位和活动组数约束。
- 数据库：SQLite/PostgreSQL 模型快照包含 `(SendingGroupId, Status, SenderAccountId, Id)` 复合索引。
- 完成后执行 `dotnet-csharpier .`、自动化测试及 CorePlugin、UzonMailDB、UzonMailService 构建。ProPlugin 当前存在本次重构前已有的 `UzonMail.*`/`UzonMail.*` 全局命名空间不一致，需在其基线修复后恢复独立构建门禁。

## 11. 明确假设

- 当前部署仍为单实例；`InMemorySendLeaseStore` 不提供跨进程互斥，但接口和租约语义必须可替换。
- 不新增消息队列或分布式缓存；数据库仍用于恢复和最终业务状态。
- 动态代理继续由 URL 识别，不新增 `ProxyType` 数据列。
- Graph 暂不支持代理；配置代理时显式报错，不忽略配置。
- 允许新增发送项复合索引迁移；除索引外不改变现有业务表字段。

## 12. 冷却与发件账户供给优化

- 工作槽每次只执行一封邮件，完成后重新参加组织、用户和发送组公平调度；冷却、日额度阻塞、失效或没有 ready 邮件的发件账户不会取得工作槽。
- 发件账户冷却改为记录 `NextEligibleUtc`，由协调器维护一个最早到期唤醒，不再由工作任务执行 `Task.Delay`；同一发件账户仍通过原子运行标记保持最多一个活动发送。
- 共享发件账户改为按 `(IsValid, Id)` 游标分页读取。默认目录页为 256，可调度候选低于 `SystemHardLimit * 2` 时触发补充，补至 `SystemHardLimit * 4`；冷却和额度阻塞账户不计入 ready 目标。
- 补页在用户和发送组之间轮转起点并均分当前缺口，目录耗尽或达到 `MaxTrackedSenderAccounts=100000` 后停止；发送组停止时通过反向组索引解绑，不扫描整个发件账户池。
- 发件账户日计数增加 UTC 日期，跨日按需归零，不再每天更新整张发件账户表。全部发件账户达到日限额时，发送组进入 `WaitingForQuotaReset`，在下一个 UTC 自然日自动恢复。
- 新增 `SendCore:SenderAccountSupply` 配置：`CatalogPageSize`、`ReadyTargetMultiplier`、`ReadyLowWatermarkMultiplier`、`MaxTrackedSenderAccounts`。
