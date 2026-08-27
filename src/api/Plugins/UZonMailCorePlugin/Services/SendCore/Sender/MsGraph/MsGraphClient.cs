using System.Net;
using System.Security.Authentication;
using log4net;
using MailKit.Net.Proxy;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Json;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

/// <summary>
/// 使用授权码流产生的委托令牌调用 Microsoft Graph。
/// </summary>
public sealed class MsGraphClient(
    ICredentialProtector credentialProtector,
    DebugConfig debugConfig,
    HttpClient httpClient
) : IMsGraphClient
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(MsGraphClient));
    private AuthenticationResult2? _authenticationResult;
    private string _authenticationFingerprint = string.Empty;
    private string _email = string.Empty;

    public IProxyClient? ProxyClient { get; set; }

    public void SetParams(string email, int cooldownMilliseconds)
    {
        _email = email;
    }

    public async Task AuthenticateAsync(
        SenderEmailAddress senderAccount,
        SqlContext db,
        CancellationToken cancellationToken = default
    )
    {
        var oauth = senderAccount.Credentials.OAuth;
        if (oauth is not { Provider: OAuthProvider.Microsoft })
            throw new AuthenticationException("Microsoft Graph 发件账户缺少 Microsoft OAuth 凭据。");
        if (string.IsNullOrWhiteSpace(oauth.ClientId))
            throw new AuthenticationException("Microsoft Graph OAuth ClientId 未配置。");
        if (string.IsNullOrWhiteSpace(oauth.RefreshToken))
            throw new AuthenticationException("Microsoft Graph 授权已失效，请重新授权。");

        var fingerprint = $"{oauth.ClientId}\n{oauth.RefreshToken}";
        if (
            _authenticationResult is { } cached
            && cached.ExpireAt > DateTime.UtcNow
            && _authenticationFingerprint == fingerprint
        )
            return;

        _authenticationFingerprint = fingerprint;
        if (
            !string.IsNullOrWhiteSpace(oauth.AccessToken)
            && oauth.AccessTokenExpiresAtUtc > DateTime.UtcNow.AddMinutes(1)
        )
        {
            _authenticationResult = new AuthenticationResult2
            {
                AccessToken = oauth.AccessToken,
                Scope = oauth.AuthorizedScopes,
                ExpiresIn = (oauth.AccessTokenExpiresAtUtc.Value - DateTime.UtcNow).TotalSeconds,
            };
            return;
        }

        var result = await RefreshAccessTokenAsync(oauth, cancellationToken);
        _authenticationResult = result;
        await PersistTokensAsync(db, oauth, result, cancellationToken);
    }

    private async Task<AuthenticationResult2> RefreshAccessTokenAsync(
        OAuthCredentialSnapshot oauth,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, string> form =
            new()
            {
                ["client_id"] = oauth.ClientId,
                ["refresh_token"] = oauth.RefreshToken,
                ["grant_type"] = "refresh_token",
                ["scope"] = oauth.AuthorizedScopes,
            };
        if (!string.IsNullOrWhiteSpace(oauth.ClientSecret))
            form["client_secret"] = oauth.ClientSecret;

        using var response = await httpClient.PostAsync(
            oauth.TokenEndpoint,
            new FormUrlEncodedContent(form),
            cancellationToken
        );
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = TryReadOAuthError(responseContent) ?? response.ReasonPhrase ?? "未知错误";
            Logger.Warn($"Microsoft Graph OAuth 刷新失败: {response.StatusCode} {message}");
            throw new AuthenticationException(message);
        }

        var result = responseContent.JsonTo<AuthenticationResult2>();
        if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
            throw new AuthenticationException("Microsoft Graph OAuth 响应缺少访问令牌。");
        if (!result.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("Mail.Send"))
            throw new AuthenticationException("Microsoft Graph 授权缺少 Mail.Send 权限。");
        return result;
    }

    private async Task PersistTokensAsync(
        SqlContext db,
        OAuthCredentialSnapshot oauth,
        AuthenticationResult2 result,
        CancellationToken cancellationToken
    )
    {
        var credential = await db.EmailAccountOAuthCredentials.FirstOrDefaultAsync(
            x => x.Id == oauth.CredentialId,
            cancellationToken
        );
        if (credential == null)
            throw new AuthenticationException("Microsoft Graph OAuth 凭据不存在。");

        var refreshToken = string.IsNullOrWhiteSpace(result.RefreshToken)
            ? oauth.RefreshToken
            : result.RefreshToken;
        var protectedAccessToken = credentialProtector.Protect(result.AccessToken);
        var protectedRefreshToken = credentialProtector.Protect(refreshToken);
        credential.EncryptedAccessToken = protectedAccessToken.Ciphertext;
        credential.EncryptedRefreshToken = protectedRefreshToken.Ciphertext;
        credential.EncryptionKeyVersion = protectedAccessToken.KeyVersion;
        credential.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
        credential.AuthorizedScopes = result.Scope;
        credential.CredentialUpdatedAtUtc = DateTime.UtcNow;

        if (
            credential.ApplicationSource == OAuthApplicationSource.Custom
            && !string.IsNullOrWhiteSpace(oauth.ClientSecret)
        )
            credential.EncryptedClientSecret = credentialProtector
                .Protect(oauth.ClientSecret)
                .Ciphertext;
        else
            credential.EncryptedClientSecret = null;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? TryReadOAuthError(string responseContent)
    {
        try
        {
            return JObject.Parse(responseContent).Value<string>("error_description");
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> SendAsync(MimeMessage mimeMessage)
    {
        if (debugConfig.PreventSending)
            return "调试模式中已阻止真实发件";

        var accessToken =
            _authenticationResult?.AccessToken
            ?? throw new AuthenticationException("发送邮件前必须先完成 OAuth2 验证。");
        var request = new MsGraphSendMailRequest()
            .WithAccessToken(accessToken)
            .WithMimeMessage(mimeMessage)
            .WithUrl("https://graph.microsoft.com/v1.0/me/sendMail")
            .WithHttpClient(httpClient);
        using var response = await request.SendAsync();
        if (response.StatusCode == HttpStatusCode.Accepted)
            return string.Empty;

        Logger.Warn($"Microsoft Graph 发件失败: {_email} {response.StatusCode}");
        throw new HttpRequestException(
            $"Microsoft Graph 发件失败：{response.ReasonPhrase}",
            null,
            response.StatusCode
        );
    }
}
