using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails;

public sealed record ProtocolCredentialInput(
    string Host,
    int Port,
    ConnectionSecurity ConnectionSecurity,
    string? LoginName,
    string? Password
);

public sealed record MicrosoftOAuthApplicationInput(
    OAuthApplicationSource ApplicationSource,
    string? TenantId,
    string? ClientId,
    string? ClientSecret
);

/// <summary>
/// 账户协议凭据的唯一写入边界。
/// </summary>
public sealed partial class AccountCredentialService(
    SqlContext db,
    ICredentialProtector credentialProtector
) : IScopedService
{
    private const string MicrosoftScopes = "openid offline_access Mail.Send Mail.Read";

    public async Task SetSmtpCredentialAsync(
        SenderAccount senderAccount,
        ProtocolCredentialInput input,
        string defaultLoginName,
        CancellationToken cancellationToken = default
    )
    {
        ValidateConnection(input);
        var credential = await db.SenderAccountSmtpCredentials.FirstOrDefaultAsync(
            x => x.SenderAccountId == senderAccount.Id,
            cancellationToken
        );
        if (credential == null)
        {
            RequireNewPassword(input.Password);
            credential = new SenderAccountSmtpCredential { SenderAccount = senderAccount };
            db.SenderAccountSmtpCredentials.Add(credential);
        }

        credential.Host = input.Host.Trim();
        credential.Port = input.Port;
        credential.ConnectionSecurity = input.ConnectionSecurity;
        credential.LoginName = ResolveLoginName(input.LoginName, defaultLoginName);
        if (input.Password != null)
        {
            RejectMaskedSecret(input.Password);
            var protectedPassword = credentialProtector.Protect(input.Password);
            credential.EncryptedPassword = protectedPassword.Ciphertext;
            credential.EncryptionKeyVersion = protectedPassword.KeyVersion;
        }
        credential.CredentialUpdatedAtUtc = DateTime.UtcNow;
        senderAccount.Status = SenderAccountStatus.Unverified;
        senderAccount.ValidationFailureReason = null;
    }

    public async Task SetImapCredentialAsync(
        ReceivingAccount receivingAccount,
        ProtocolCredentialInput input,
        string defaultLoginName,
        CancellationToken cancellationToken = default
    )
    {
        ValidateConnection(input);
        if (receivingAccount.AuthenticationMethod != AuthenticationMethod.Password)
            throw new KnownException("OAuth2 IMAP 凭据暂未开放");

        var credential = await db.ReceivingAccountImapCredentials.FirstOrDefaultAsync(
            x => x.ReceivingAccountId == receivingAccount.Id,
            cancellationToken
        );
        if (credential == null)
        {
            RequireNewPassword(input.Password);
            credential = new ReceivingAccountImapCredential
            {
                ReceivingAccount = receivingAccount,
            };
            db.ReceivingAccountImapCredentials.Add(credential);
        }

        credential.Host = input.Host.Trim();
        credential.Port = input.Port;
        credential.ConnectionSecurity = input.ConnectionSecurity;
        credential.LoginName = ResolveLoginName(input.LoginName, defaultLoginName);
        if (input.Password != null)
        {
            RejectMaskedSecret(input.Password);
            var protectedPassword = credentialProtector.Protect(input.Password);
            credential.EncryptedPassword = protectedPassword.Ciphertext;
            credential.EncryptionKeyVersion = protectedPassword.KeyVersion;
        }
        credential.CredentialUpdatedAtUtc = DateTime.UtcNow;
        receivingAccount.Status = ReceivingAccountStatus.Unverified;
        receivingAccount.LastError = null;
    }

    public async Task<EmailAccountOAuthCredential> ConfigureMicrosoftOAuthAsync(
        EmailAccount emailAccount,
        MicrosoftOAuthApplicationInput input,
        CancellationToken cancellationToken = default
    )
    {
        var credential = await db.EmailAccountOAuthCredentials.FirstOrDefaultAsync(
            x => x.EmailAccountId == emailAccount.Id,
            cancellationToken
        );
        if (credential == null)
        {
            credential = new EmailAccountOAuthCredential { EmailAccount = emailAccount };
            db.EmailAccountOAuthCredentials.Add(credential);
        }

        credential.Provider = OAuthProvider.Microsoft;
        credential.ApplicationSource = input.ApplicationSource;
        credential.AuthorizedScopes = MicrosoftScopes;
        credential.EncryptedAccessToken = null;
        credential.EncryptedRefreshToken = null;
        credential.AccessTokenExpiresAtUtc = null;
        credential.EncryptionKeyVersion = credentialProtector.ActiveKeyVersion;
        credential.CredentialUpdatedAtUtc = DateTime.UtcNow;

        if (input.ApplicationSource == OAuthApplicationSource.System)
        {
            credential.TenantId = "common";
            credential.ClientId = null;
            credential.EncryptedClientSecret = null;
            credential.TokenEndpoint = GetTokenEndpoint("common");
            return credential;
        }

        if (string.IsNullOrWhiteSpace(input.ClientId))
            throw new KnownException("自定义 Microsoft Entra 应用必须提供 ClientId");
        credential.TenantId = string.IsNullOrWhiteSpace(input.TenantId)
            ? "common"
            : input.TenantId.Trim();
        credential.ClientId = input.ClientId.Trim();
        credential.TokenEndpoint = GetTokenEndpoint(credential.TenantId);
        if (input.ClientSecret != null)
        {
            RejectMaskedSecret(input.ClientSecret);
            credential.EncryptedClientSecret = credentialProtector
                .Protect(input.ClientSecret)
                .Ciphertext;
        }
        else
        {
            credential.EncryptedClientSecret = null;
        }
        return credential;
    }

    private static void ValidateConnection(ProtocolCredentialInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Host))
            throw new KnownException("服务器地址不能为空");
        if (input.Port is <= 0 or > 65535)
            throw new KnownException("端口必须在 1 到 65535 之间");
    }

    private static string ResolveLoginName(string? loginName, string defaultLoginName)
    {
        var resolvedLoginName = string.IsNullOrWhiteSpace(loginName)
            ? defaultLoginName.Trim()
            : loginName.Trim();
        if (string.IsNullOrWhiteSpace(resolvedLoginName))
            throw new KnownException("邮箱地址不能为空");
        return resolvedLoginName;
    }

    private static void RequireNewPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            throw new KnownException("新建协议凭据必须提供密码");
    }

    private static void RejectMaskedSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret))
            throw new KnownException("凭据不能为空");
        if (MaskRegex().IsMatch(secret))
            throw new KnownException("密码掩码不能作为实际凭据保存");
    }

    private static string GetTokenEndpoint(string tenantId) =>
        $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/token";

    [GeneratedRegex("^[*•]+$")]
    private static partial Regex MaskRegex();
}
