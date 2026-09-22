# api 项目规范

## 交付目标

- 多组织、多用户
- 单进程内高并发发件、高吞吐量、高性能
- 后台可能同时存在几千发件箱，上百万的邮箱待发送

## 服务注册

- 非泛型接口自动注册：所有实现 IScopedService、ISingletonService 或 ITransientService 的实现类型，都会自动注册为自身的服务类型
- 泛型接口自动注册：所有实现 IScopedService<T>、ISingletonService<T> 或 ITransientService<T> 的实现类型，都会自动注册为自身以及 T 对应的服务类型

## 配置调用

- 业务配置类型实现 `IAppOptions` 标记接口，由 `AddAllOptions` 自动扫描并注册，不要手动调用 `services.Configure<T>`
- 使用 `IOptions<T>` 获取固定配置，使用 `IOptionsSnapshot<T>` 或 `IOptionsMonitor<T>` 获取支持热更新的配置

## 控制器

- 所有的控制器继承 ControllerBaseV1 类
- 请求模型保存到控制器所在目录下的 ./DTOs/ 目录中，请示模型不要直接使用数据库实体类
- 简单验证使用 Data Annotations 标注，复杂验证使用 FluentValidation 验证

## 数据库

- 优先考虑逻辑删除
- 删除数据时，禁止使用级联删除，所有删除操作在应用层手动分步删除
- 数据库迁移使用类似这样 `dotnet ef migrations add xxx --context MysqlContext --output-dir Migrations/Mysql -v` 的命令进行自动迁移，禁止只手写 `Up/Down`，否则运行时 `Database.Migrate()` 会因 `PendingModelChangesWarning` 失败
- 修改实体模型或生成迁移后，分别对受影响的 Context 执行 `dotnet ef migrations has-pending-model-changes --context <ContextName>` 验证；不得通过忽略或抑制该警告绕过模型快照不一致

## EF Core

在编写任何 LINQ 或 EF Core 查询代码时，你必须严格遵守以下规范：

1. **只读必无追踪**：只要没有后续修改需求，查询末尾必须加 `.AsNoTracking()`。
2. **严禁 N+1**：绝对不要在任何循环中、或 Select 隐式嵌套中编写数据库查询。
3. **按需投影**：涉及多表关联或宽表查询，必须使用 `.Select()` 投影到局部匿名对象或特定的 DTO，拒绝返回整个实体模型。
4. **存在性检查**：检查是否存在一律使用 `.AnyAsync()`，不许使用 Count 或 FirstOrDefault。
5. **批量操作优化**：如果是大批量更新/删除，必须使用 `ExecuteUpdateAsync` 和 `ExecuteDeleteAsync`。

### Include 约束

- 不要超过 2 个“一对多（Collection）”分支
- 深度不超过 3 层
- 禁止在面向前API的查询中使用富 Include，必须使用 Select 或分步查询

## 项目依赖

- UzonMailProPlugin 依赖于 UzonMailCorePlugin, 后者不能关联任何前者中的逻辑
