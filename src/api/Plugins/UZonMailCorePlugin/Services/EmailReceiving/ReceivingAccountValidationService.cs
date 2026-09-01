using System.Net.Http.Headers;
using System.Security.Authentication;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

/// <summary>
/// 验证收件账户配置与认证，不执行邮件同步。
/// </summary>
public sealed class ReceivingAccountValidationService(
    SqlContext db,
    ICredentialProtector credentialProtector,
    HttpClient httpClient
) : IScopedService
{
    public async Task ValidateAsync(
        long userId,
        long receivingAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var account =
            await db
                .ReceivingAccounts.Include(x => x.EmailAccount)
                .ThenInclude(x => x.OAuthCredential)
                .FirstOrDefaultAsync(
                    x => x.Id == receivingAccountId && x.EmailAccount.UserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("收件账户不存在");

        if (
            account.Protocol == ReceivingProtocol.Imap
            && !await db.ReceivingAccountImapCredentials.AnyAsync(
                x => x.ReceivingAccountId == account.Id,
                cancellationToken
            )
        )
        {
            account.Status = ReceivingAccountStatus.ConfigurationRequired;
            account.LastSyncAttemptAtUtc = DateTime.UtcNow;
            account.LastError = "IMAP 凭据未配置";
            await db.SaveChangesAsync(cancellationToken);
            throw new KnownException(account.LastError);
        }

        account.LastSyncAttemptAtUtc = DateTime.UtcNow;
        try
        {
            if (account.Protocol == ReceivingProtocol.Imap)
                await ValidateImapAsync(account, cancellationToken);
            else if (account.Protocol == ReceivingProtocol.MicrosoftGraph)
                await ValidateMicrosoftGraphAsync(account, cancellationToken);
            else
                throw new KnownException("不支持的收件协议");

            account.Status = ReceivingAccountStatus.Active;
            account.LastConnectedAtUtc = DateTime.UtcNow;
            account.LastError = null;
        }
        catch (Exception exception)
        {
            account.Status = exception is AuthenticationException or KnownException
                ? ReceivingAccountStatus.AuthenticationFailed
                : ReceivingAccountStatus.ConnectionFailed;
            account.LastError = exception.Message;
            await db.SaveChangesAsync(cancellationToken);
            throw new KnownException(exception.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateImapAsync(
        ReceivingAccount account,
        CancellationToken cancellationToken
    )
    {
        if (account.AuthenticationMethod != AuthenticationMethod.Password)
            throw new KnownException("通用 IMAP OAuth2 暂未开放");
        var credential =
            await db.ReceivingAccountImapCredentials.FirstOrDefaultAsync(
                x => x.ReceivingAccountId == account.Id,
                cancellationToken
            ) ?? throw new KnownException("IMAP 凭据未配置");
        if (
            string.IsNullOrWhiteSpace(credential.EncryptedPassword)
            || string.IsNullOrWhiteSpace(credential.EncryptionKeyVersion)
        )
            throw new KnownException("IMAP Password 凭据不完整");

        var password = credentialProtector.Unprotect(
            credential.EncryptedPassword,
            credential.EncryptionKeyVersion
        );
        using var client = new ImapClient();
        await client.ConnectAsync(
            credential.Host,
            credential.Port,
            credential.ConnectionSecurity.ToMailKitSecureSocketOptions(),
            cancellationToken
        );
        await client.AuthenticateAsync(credential.LoginName, password, cancellationToken);
        await ImapClientIdentification.IdentifyAsync(client, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task ValidateMicrosoftGraphAsync(
        ReceivingAccount account,
        CancellationToken cancellationToken
    )
    {
        if (account.AuthenticationMethod != AuthenticationMethod.OAuth2)
            throw new KnownException("Microsoft Graph 收件账户必须使用 OAuth2");
        var credential = account.EmailAccount.OAuthCredential;
        if (
            credential is not { Provider: OAuthProvider.Microsoft }
            || string.IsNullOrWhiteSpace(credential.EncryptedAccessToken)
            || credential.AccessTokenExpiresAtUtc <= DateTime.UtcNow
        )
            throw new KnownException("Microsoft Graph 授权无效或已过期，请重新授权");

        var accessToken = credentialProtector.Unprotect(
            credential.EncryptedAccessToken,
            credential.EncryptionKeyVersion
        );
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://graph.microsoft.com/v1.0/me?$select=id"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new AuthenticationException($"Microsoft Graph 认证失败：{(int)response.StatusCode}");
    }
}
