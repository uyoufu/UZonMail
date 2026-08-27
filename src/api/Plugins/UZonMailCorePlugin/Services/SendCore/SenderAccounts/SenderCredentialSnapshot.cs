using MailKit.Security;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

/// <summary>
/// SMTP 密钥解密后的短生命周期快照，仅存在于发送运行时。
/// </summary>
public sealed record SmtpCredentialSnapshot(
    string Host,
    int Port,
    ConnectionSecurity ConnectionSecurity,
    string LoginName,
    string Password
);

/// <summary>
/// Microsoft Graph 委托授权的短生命周期快照。
/// </summary>
public sealed record OAuthCredentialSnapshot(
    long CredentialId,
    OAuthProvider Provider,
    OAuthApplicationSource ApplicationSource,
    string TenantId,
    string ClientId,
    string? ClientSecret,
    string? AccessToken,
    string RefreshToken,
    DateTime? AccessTokenExpiresAtUtc,
    string AuthorizedScopes,
    string TokenEndpoint
);

public sealed record SenderCredentialSnapshot(
    SmtpCredentialSnapshot? Smtp,
    OAuthCredentialSnapshot? OAuth
);
