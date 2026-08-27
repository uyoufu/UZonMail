using MailKit.Security;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs;

public sealed record SenderAccountDto(
    long Id,
    long EmailAccountId,
    long EmailGroupId,
    string Email,
    string? Name,
    string? Description,
    string? Remark,
    SendingProtocol Protocol,
    AuthenticationMethod AuthenticationMethod,
    SenderAccountStatus Status,
    string? ValidationFailureReason,
    long? ProxyId,
    int MaxSendCountPerDay,
    int SentTotalToday,
    string? ReplyToEmails,
    int Weight,
    bool HasSmtpCredential,
    OAuthApplicationSource? OAuthApplicationSource,
    bool HasOAuthAuthorization
);

public sealed class CreateSmtpSenderAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public long EmailGroupId { get; set; }
    public long? ProxyId { get; set; }
    public int MaxSendCountPerDay { get; set; }
    public string? ReplyToEmails { get; set; }
    public int Weight { get; set; } = 1;
    public SmtpCredentialWriteDto Credential { get; set; } = new();
}

public sealed class CreateMicrosoftGraphSenderAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public long EmailGroupId { get; set; }
    public int MaxSendCountPerDay { get; set; }
    public string? ReplyToEmails { get; set; }
    public int Weight { get; set; } = 1;
    public MicrosoftGraphApplicationWriteDto Application { get; set; } = new();
}

public sealed class UpdateSenderAccountDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public long EmailGroupId { get; set; }
    public long? ProxyId { get; set; }
    public int MaxSendCountPerDay { get; set; }
    public string? ReplyToEmails { get; set; }
    public int Weight { get; set; } = 1;
}

public sealed class SmtpCredentialWriteDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 465;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
    public string LoginName { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public sealed class MicrosoftGraphApplicationWriteDto
{
    public OAuthApplicationSource ApplicationSource { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
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

public sealed record ReceivingAccountDto(
    long Id,
    long EmailAccountId,
    string Email,
    string? Name,
    ReceivingProtocol Protocol,
    AuthenticationMethod AuthenticationMethod,
    ReceivingAccountStatus Status,
    int ContentRetentionDays,
    DateTime? LastConnectedAtUtc,
    string? LastError,
    bool HasImapCredential,
    OAuthApplicationSource? OAuthApplicationSource,
    bool HasOAuthAuthorization,
    IReadOnlyList<long> SenderAccountIds,
    long? PrimarySenderAccountId
);

public sealed class CreateBasicReceivingAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public int ContentRetentionDays { get; set; } = 30;
    public ImapCredentialWriteDto Credential { get; set; } = new();
    public List<long> SenderAccountIds { get; set; } = [];
    public long? PrimarySenderAccountId { get; set; }
}

public sealed class CreateMicrosoftGraphReceivingAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public int ContentRetentionDays { get; set; } = 30;
    public MicrosoftGraphApplicationWriteDto Application { get; set; } = new();
    public List<long> SenderAccountIds { get; set; } = [];
    public long? PrimarySenderAccountId { get; set; }
}

public sealed class UpdateReceivingAccountDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public int ContentRetentionDays { get; set; } = 30;
    public List<long> SenderAccountIds { get; set; } = [];
    public long? PrimarySenderAccountId { get; set; }
}

public sealed class ImapCredentialWriteDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
    public string LoginName { get; set; } = string.Empty;
    public string? Password { get; set; }
}
