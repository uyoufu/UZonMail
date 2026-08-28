using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs;

/// <summary>
/// 账户配置使用的协议族。一个邮箱身份中的发送、收件能力必须属于同一协议族，
/// 这样 Microsoft Graph 的 OAuth 授权可以被两个能力安全复用。
/// </summary>
public enum EmailAccountConfigurationKind
{
    Basic = 1,
    MicrosoftGraph = 2,
}

/// <summary>
/// 邮箱身份的非敏感展示数据。
/// </summary>
public sealed record EmailAccountDto(
    long Id,
    long EmailGroupId,
    string Email,
    string? Name,
    string? Description,
    string? Remark,
    EmailAccountSenderCapabilityDto? Sender,
    EmailAccountReceivingCapabilityDto? Receiving,
    OAuthApplicationSource? OAuthApplicationSource,
    bool HasOAuthAuthorization
);

public sealed record EmailAccountSenderCapabilityDto(
    long Id,
    SendingProtocol Protocol,
    SenderAccountStatus Status,
    string? ValidationFailureReason,
    long? ProxyId,
    int MaxSendCountPerDay,
    int SentTotalToday,
    string? ReplyToEmails,
    bool HasCredential,
    EmailAccountProtocolCredentialDto? SmtpCredential
);

public sealed record EmailAccountReceivingCapabilityDto(
    long Id,
    ReceivingProtocol Protocol,
    ReceivingAccountStatus Status,
    int ContentRetentionDays,
    DateTime? LastConnectedAtUtc,
    string? LastError,
    bool HasCredential,
    EmailAccountProtocolCredentialDto? ImapCredential
);

/// <summary>
/// 账户编辑所需的协议连接参数。密码等机密始终不出现在读取响应中。
/// </summary>
public sealed record EmailAccountProtocolCredentialDto(
    string Host,
    int Port,
    ConnectionSecurity ConnectionSecurity,
    string LoginName
);

/// <summary>
/// 发送任务选择器需要的发送能力摘要。
/// </summary>
public sealed record SenderEmailAccountOptionDto(
    long Id,
    string Email,
    string? Name,
    string? Description
);

/// <summary>
/// 创建或更新邮箱身份及其能力。密钥字段仅用于写入，响应不会回显它们。
/// </summary>
public sealed class EmailAccountWriteDto
{
    public string? Email { get; set; }
    public long EmailGroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public EmailAccountConfigurationKind ConfigurationKind { get; set; }
    public EmailAccountSenderCapabilityWriteDto Sender { get; set; } = new();
    public EmailAccountReceivingCapabilityWriteDto Receiving { get; set; } = new();
    public MicrosoftGraphApplicationWriteDto? MicrosoftGraphApplication { get; set; }
}

public sealed class EmailAccountSenderCapabilityWriteDto
{
    public bool IsEnabled { get; set; }
    public long? ProxyId { get; set; }
    public int MaxSendCountPerDay { get; set; }
    public string? ReplyToEmails { get; set; }
    public SmtpCredentialWriteDto? SmtpCredential { get; set; }
}

public sealed class EmailAccountReceivingCapabilityWriteDto
{
    public bool IsEnabled { get; set; }
    public int ContentRetentionDays { get; set; } = 30;
    public ImapCredentialWriteDto? ImapCredential { get; set; }
}

public sealed class SmtpCredentialWriteDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 465;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
    public string? LoginName { get; set; }
    public string? Password { get; set; }
}

public sealed class ImapCredentialWriteDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
    public string? LoginName { get; set; }
    public string? Password { get; set; }
}

public sealed class MicrosoftGraphApplicationWriteDto
{
    public OAuthApplicationSource ApplicationSource { get; set; } = OAuthApplicationSource.System;
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}

public sealed class MoveEmailAccountsDto
{
    public List<long> EmailAccountIds { get; set; } = [];
    public long TargetEmailGroupId { get; set; }
}

public sealed class ValidateEmailAccountsDto
{
    public List<long> EmailAccountIds { get; set; } = [];
}

public sealed record RecipientContactDto(
    long Id,
    long EmailGroupId,
    string Email,
    string? Name,
    string? Description,
    string? Remark,
    long MinimumCooldownHours,
    RecipientValidationStatus ValidationStatus,
    string? ValidationFailureReason,
    DateTime LastDeliveredAtUtc,
    DateTime LastSuccessDeliveryDate
);

public class CreateRecipientContactDto
{
    public long EmailGroupId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public long MinimumCooldownHours { get; set; } = -1;
}

public sealed class UpdateRecipientContactDto : CreateRecipientContactDto;
