# 重构邮件账户数据库

## 重构目标

- 统一数据库、API 和 Web 端的邮件账户领域命名，消除 `EmailBox`、`Inbox`、`Outbox` 带来的职责歧义。
- 区分用户拥有的邮箱账户、邮箱的发送/接收能力，以及作为投递目标的收件人联系人。
- 将密码、OAuth 密钥和令牌从账户主表中隔离，普通账户查询不得读取或返回密钥。
- 支持 IMAP Basic 和 Microsoft Graph 收件认证，并为通用 OAuth 收件认证预留数据模型。
- 保留现有数据，分别为 SQLite 和 PostgreSQL 生成规范的 EF Core 迁移。

## 领域模型

统一使用单数 PascalCase 命名 C# 实体，使用复数 PascalCase 命名物理数据表。

| 业务概念 | C# 实体 | 数据表 | 职责 |
| --- | --- | --- | --- |
| 用户拥有的邮箱身份 | `EmailAccount` | `EmailAccounts` | 保存邮箱地址及用户、组织等公共身份信息 |
| 邮箱的发信能力 | `SenderAccount` | `SenderAccounts` | 保存发送协议、状态、限额、代理和发送策略 |
| 邮箱的收信能力 | `ReceivingAccount` | `ReceivingAccounts` | 保存接收协议、状态和同步策略 |
| 群发目标联系人 | `RecipientContact` | `RecipientContacts` | 保存收件人地址、投递状态和冷却策略 |

关系使用组合而不是实体继承：

```text
EmailAccount
 ├─ 0..1 SenderAccount
 │        └─ 0..1 SenderAccountSmtpCredential
 ├─ 0..1 ReceivingAccount
 │        └─ 0..1 ReceivingAccountImapCredential
 └─ 0..1 EmailAccountOAuthCredential

RecipientContact 独立存在，不属于 EmailAccount
```

- 同一个 `EmailAccount` 可以同时具备发送和接收能力。
- `SenderAccount.EmailAccountId` 和 `ReceivingAccount.EmailAccountId` 均建立唯一索引。
- `EmailAccount` 按当前用户和规范化邮箱地址保持唯一，避免同一用户重复维护相同邮箱身份。
- `RecipientContact` 是邮件投递目标，不具备登录或认证语义，不使用 `RecipientAccount` 或 `ContactAccount` 命名。

## 现有类型重命名

当前涉及的类型位于 `src/api/UZonMailDB/SQL/Core/Emails/`。

| 当前名称 | 目标名称或处理方式 |
| --- | --- |
| `EmailBox` | 改造为真实实体 `EmailAccount`，不再作为 `Inbox` 和 `Outbox` 的公共基类 |
| `EmailBoxStatus` | 删除；生命周期继续使用 `IsDeleted`，能力状态由对应实体维护 |
| `EmailBoxType` | 删除；由 `SenderAccount`、`ReceivingAccount` 能力表表达类型 |
| `Inbox` | `RecipientContact` |
| `InboxStatus` | `RecipientValidationStatus` |
| `Outbox` | 拆分为 `EmailAccount`、`SenderAccount` 和对应凭证表 |
| `OutboxStatus` | `SenderAccountStatus` |
| `OutboxType` | `SendingProtocol` |
| `OutboxSetting` | 删除；当前未注册为 `DbSet` 且没有调用方，代理配置继续使用现有 `Proxy` 模型 |
| `ImapAccount` | `ReceivingAccount` |
| `ImapAccountCredential` | 重构为 `ReceivingAccountImapCredential`，OAuth 密钥迁移到账户级 OAuth 凭证 |

`EmailGroups` 表保留，`EmailGroupType` 改为 `EmailGroupCategory`，枚举值使用 `Sender` 和 `Recipient`。`SenderAccount` 与 `RecipientContact` 分别保存所属分组外键。

## 发件账户 SMTP 凭证

SMTP 连接参数和密码具有相同生命周期，发送和验证时始终一起读取，因此合并到一张协议凭证表：

- C# 实体：`SenderAccountSmtpCredential`
- 数据表：`SenderAccountSmtpCredentials`
- 外键：`SenderAccountId`，建立唯一索引，形成一对一关系
- 连接字段：`Host`、`Port`、`ConnectionSecurity`、`LoginName`
- 密钥字段：`EncryptedPassword`、`EncryptionKeyVersion`、`CredentialUpdatedAtUtc`

该表只允许在 SMTP 发送、账户验证和凭证维护场景中加载。普通发件账户列表、统计查询和常规 API DTO 不得关联或返回该表的密钥字段。

## 收件账户认证

### IMAP

IMAP 连接参数和 Basic 密码在连接、验证及维护时具有相同生命周期，因此合并到一张协议凭证表：

- C# 实体：`ReceivingAccountImapCredential`
- 数据表：`ReceivingAccountImapCredentials`
- 外键：`ReceivingAccountId`，建立唯一索引，形成一对一关系
- 连接字段：`Host`、`Port`、`ConnectionSecurity`、`LoginName`
- 密钥字段：`EncryptedPassword`、`EncryptionKeyVersion`、`CredentialUpdatedAtUtc`

使用 Password 认证时，`EncryptedPassword` 和 `EncryptionKeyVersion` 必填。使用 OAuth2 认证时，连接仍复用该表的 Host、Port、ConnectionSecurity 和 LoginName，但密码及其密钥版本必须为空，实际 OAuth 密钥从 `EmailAccountOAuthCredential` 读取。

该表只允许在 IMAP 同步、账户验证和凭证维护场景中加载。普通收件账户查询和 API DTO 不得返回密码字段。

### OAuth

OAuth 凭证归 `EmailAccount` 所有，由该邮箱的发送和接收能力共享：

- C# 实体：`EmailAccountOAuthCredential`
- 数据表：`EmailAccountOAuthCredentials`
- 外键：`EmailAccountId`，建立唯一索引
- 字段包括 `Provider`、`TenantId`、`ClientId`、加密后的 Client Secret、Access Token、Refresh Token、访问令牌过期时间、授权范围、令牌端点、密钥版本和凭证更新时间

发送和收件共享 OAuth 授权时，授权范围必须包含两种能力所需权限的并集。任何 Client Secret、Access Token 和 Refresh Token 均只允许以加密形式持久化。

## 协议与认证枚举

使用强类型枚举表达协议和认证方式，禁止以 `Basic`、`MsGraph` 等魔法字符串驱动业务逻辑：

- `SendingProtocol`：`Smtp`、`MicrosoftGraph`
- `ReceivingProtocol`：`Imap`、`MicrosoftGraph`
- `AuthenticationMethod`：`Password`、`OAuth2`
- `OAuthProvider`：`Generic`、`Google`、`Microsoft`

收件账户的创建方式与数据组合固定为：

| Web 入口 | 接收协议 | 认证方式 | 本期状态 |
| --- | --- | --- | --- |
| Basic | `Imap` | `Password` | 可用 |
| MsGraph | `MicrosoftGraph` | `OAuth2` | 可用 |
| OAuth | `Imap` | `OAuth2` | 仅预留模型和接口契约，暂不实现通用 OAuth 流程 |

## 收发账户关联

一个收件账户可以为多个发件账户收集和归因回信，现有 IMAP 专属关联应提升为协议无关命名：

- `ImapAccountOutboxLinks` -> `ReceivingAccountSenderLinks`
- `ImapAccountPrimaryOutboxes` -> `ReceivingAccountPrimarySenders`

`ReceivingAccountSenderLinks` 对 `ReceivingAccountId + SenderAccountId` 建立唯一索引。继续使用独立的 `ReceivingAccountPrimarySender` 实体约束每个收件账户最多存在一个主要发件账户，不使用级联删除。

协议专属表继续保留协议前缀，例如 `ImapMailboxes`、`ImapSyncRuns` 和 `ImapSyncCommands`，但其账户外键统一从 `ImapAccountId` 改为 `ReceivingAccountId`。通用的 `IncomingMail*` 表保持名称，并将关联账户字段改为 `ReceivingAccountId`。

## 发送任务相关命名

同步重构数据库实体、外键、DTO 和业务变量中的旧命名：

- `SendingItemInboxes` -> `SendingItemRecipients`
- `OutboxSendingGroup` -> `SenderAccountSendingGroup`
- `OutboxId` / `outboxId` -> `SenderAccountId` / `senderAccountId`
- `InboxId` / `inboxId` -> `RecipientContactId` / `recipientContactId`
- `OutboxEmail` / `outboxEmail` -> `SenderEmail` / `senderEmail`
- `InboxEmail` / `inboxEmail` -> `RecipientEmail` / `recipientEmail`
- `OutboxesCount` -> `SenderAccountCount`
- `InboxesCount` -> `RecipientCount`

导入导出模型使用 `senderEmail`、`recipientEmail`、`senderLoginName` 等具有业务含义的字段，不再新增包含 `outbox`、`inbox` 的公共字段。

## API 与 Web 端

### API

- 使用独立请求和响应 DTO，不直接返回数据库实体。
- 资源路径统一使用 `/sender-accounts`、`/recipient-contacts` 和 `/receiving-accounts`。
- 常规查询只返回账户身份、状态和非敏感配置；密码、Client Secret 和 Token 不得出现在响应中。
- 凭证新增和更新使用独立请求模型，密码掩码不得作为实际凭证写回数据库。

### Web

- `SenderAccount` 对应“发件人”或“发件账户”。
- `RecipientContact` 对应“收件人”。
- `ReceivingAccount` 对应“收件账户”，禁止与“收件人”混用。
- 收件账户页面的新建按钮使用 `q-fab` 和 `q-fab-action`，提供 Basic、MsGraph、OAuth 三种入口。
- Basic 和 MsGraph 入口可用；通用 OAuth 入口显示但保持禁用。
- 收件人联系人继续使用普通新增和导入流程，不展示认证方式。

## 数据迁移

- 当前仓库已经存在 `ImapAccounts`、`ImapAccountCredentials` 及相关迁移，应重构并迁移这些表，不得再增加一套含义重叠的收件账户模型。
- 从现有 `Outboxes` 和 `ImapAccounts` 中按用户及规范化邮箱地址生成 `EmailAccounts`，再创建对应的 `SenderAccounts` 和 `ReceivingAccounts`。
- 将 SMTP 连接字段和加密密码迁移到 `SenderAccountSmtpCredentials`。
- 将现有 IMAP 连接字段和加密密码迁移到 `ReceivingAccountImapCredentials`，OAuth 字段迁移到 `EmailAccountOAuthCredentials`。
- 迁移过程必须保留现有密文，不得在迁移日志或异常信息中输出明文凭证。
- 数据迁移完成并验证后，才能移除旧表和旧字段。
- 分别通过 EF Core 为 SQLite 和 PostgreSQL 自动生成迁移，禁止只手写 `Up` / `Down`。

## 测试与验收

- 测试 `EmailAccount` 与发送、收件能力的一对一约束及邮箱唯一性。
- 测试 SMTP、IMAP 协议凭证表的一对一约束和原子更新。
- 测试 IMAP Password 认证必须提供加密密码，OAuth2 认证不得保留密码字段。
- 测试 Basic、MsGraph 对应的协议与认证组合，拒绝不合法组合。
- 测试收件账户与发件账户的多对多关系及唯一主要发件账户约束。
- 测试普通账户 API、日志和导出文件不包含密码、Client Secret、Access Token 或 Refresh Token。
- 测试旧数据、状态、分组、关联和加密凭证能够无损迁移。
- 测试 Web 端的发件人、收件人和收件账户标签以及 q-fab 可用状态。
- 分别对受影响的 Context 执行 `dotnet ef migrations has-pending-model-changes`。
- 在仓库根目录执行：

```powershell
dotnet test --project tests/UzonMailDotNET.Test/UzonMailDotNET.Test.csproj
```
