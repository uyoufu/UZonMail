using UzonMail.CorePlugin.Config.SubConfigs;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

/// <summary>
/// 将持久化账户转换为包含明文凭据的短生命周期发送对象。
/// </summary>
public sealed class SenderAccountRuntimeFactory(
    ICredentialProtector credentialProtector,
    IConfiguration configuration
) : IScopedService
{
    private const string DefaultTokenEndpoint =
        "https://login.microsoftonline.com/common/oauth2/v2.0/token";

    public SenderEmailAddress Create(
        SenderAccount senderAccount,
        long sendingGroupId,
        SenderEmailAddressType addressType,
        List<long>? sendingItemIds = null
    )
    {
        var credentials = new SenderCredentialSnapshot(
            CreateSmtpSnapshot(senderAccount.SmtpCredential),
            CreateOAuthSnapshot(senderAccount.EmailAccount.OAuthCredential)
        );
        return new SenderEmailAddress(
            senderAccount,
            credentials,
            sendingGroupId,
            addressType,
            sendingItemIds
        );
    }

    private SmtpCredentialSnapshot? CreateSmtpSnapshot(SenderAccountSmtpCredential? credential)
    {
        if (credential == null)
            return null;

        return new SmtpCredentialSnapshot(
            credential.Host,
            credential.Port,
            credential.ConnectionSecurity,
            credential.LoginName,
            credentialProtector.Unprotect(
                credential.EncryptedPassword,
                credential.EncryptionKeyVersion
            )
        );
    }

    private OAuthCredentialSnapshot? CreateOAuthSnapshot(EmailAccountOAuthCredential? credential)
    {
        if (credential == null)
            return null;

        var clientId = credential.ClientId ?? string.Empty;
        var tenantId = credential.TenantId ?? "common";
        string? clientSecret = UnprotectOptional(
            credential.EncryptedClientSecret,
            credential.EncryptionKeyVersion
        );
        if (credential.ApplicationSource == OAuthApplicationSource.System)
        {
            var systemApplication = new MicrosoftEntraAppOptions();
            configuration.GetRequiredSection("MicrosoftEntraApp").Bind(systemApplication);
            clientId = systemApplication.ClientId;
            tenantId = string.IsNullOrWhiteSpace(systemApplication.TenantId)
                ? "common"
                : systemApplication.TenantId;
            clientSecret = string.IsNullOrWhiteSpace(systemApplication.ClientSecret)
                ? null
                : systemApplication.ClientSecret;
        }

        return new OAuthCredentialSnapshot(
            credential.Id,
            credential.Provider,
            credential.ApplicationSource,
            tenantId,
            clientId,
            clientSecret,
            UnprotectOptional(credential.EncryptedAccessToken, credential.EncryptionKeyVersion),
            UnprotectOptional(credential.EncryptedRefreshToken, credential.EncryptionKeyVersion)
                ?? string.Empty,
            credential.AccessTokenExpiresAtUtc,
            credential.AuthorizedScopes,
            string.IsNullOrWhiteSpace(credential.TokenEndpoint)
                ? DefaultTokenEndpoint
                : credential.TokenEndpoint
        );
    }

    private string? UnprotectOptional(string? ciphertext, string keyVersion)
    {
        return string.IsNullOrWhiteSpace(ciphertext)
            ? null
            : credentialProtector.Unprotect(ciphertext, keyVersion);
    }
}
