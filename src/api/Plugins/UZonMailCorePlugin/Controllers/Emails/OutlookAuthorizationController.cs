using System.Reflection;
using System.Security.Authentication;
using System.Text.Json;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Config.SubConfigs;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Json;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails;

/// <summary>
/// Microsoft Graph 授权码流入口。
/// </summary>
public sealed class OutlookAuthorizationController(
    SqlContext db,
    TokenService tokenService,
    ICredentialProtector credentialProtector,
    IDataProtectionProvider dataProtectionProvider,
    IConfiguration configuration,
    HttpClient httpClient
) : ControllerBaseV1
{
    private static readonly ILog Logger = LogManager.GetLogger(
        typeof(OutlookAuthorizationController)
    );
    private static readonly string[] AuthorizationScopes =
    [
        "openid",
        "offline_access",
        "Mail.Send",
        "Mail.Read",
    ];
    private static string _callbackPage = string.Empty;
    private readonly IDataProtector _stateProtector = dataProtectionProvider.CreateProtector(
        "UzonMail.MicrosoftGraph.AuthorizationState.v1"
    );

    /// <summary>
    /// 为仅配置收件能力的邮箱身份发起同一授权码流。
    /// </summary>
    [HttpPost("email-accounts/{emailAccountId:long}")]
    public async Task<ResponseResult<string>> OnEmailAccountAuthorizationRequest(
        long emailAccountId
    )
    {
        var userId = tokenService.GetUserSqlId();
        var emailAccount =
            await db
                .EmailAccounts.Include(x => x.OAuthCredential)
                .FirstOrDefaultAsync(x => x.Id == emailAccountId && x.UserId == userId)
            ?? throw new KnownException("未找到邮箱账户");
        return (await BuildAuthorizationUrlAsync(emailAccount, userId)).ToSuccessResponse();
    }

    private async Task<string> BuildAuthorizationUrlAsync(EmailAccount emailAccount, long userId)
    {
        var oauthCredential = await EnsureMicrosoftCredentialAsync(emailAccount);
        var application = ResolveApplication(oauthCredential);
        var state = _stateProtector.Protect(
            JsonSerializer.Serialize(
                new AuthorizationState(emailAccount.Id, userId, DateTime.UtcNow.AddMinutes(10))
            )
        );
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = application.ClientId,
            ["redirect_uri"] = GetRedirectUri(),
            ["response_type"] = "code",
            ["response_mode"] = "query",
            ["scope"] = string.Join(' ', AuthorizationScopes),
            ["state"] = state,
            ["login_hint"] = emailAccount.Email,
            ["prompt"] = "select_account",
        };
        var authority =
            $"https://login.microsoftonline.com/{Uri.EscapeDataString(application.TenantId)}/oauth2/v2.0/authorize";
        return BuildUri(authority, query);
    }

    [AllowAnonymous]
    [HttpGet("code")]
    public async Task<IActionResult> OnAuthorizationCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery(Name = "error_description")] string? errorDescription
    )
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(error))
                return await CreateCallbackResultAsync(false, errorDescription ?? error);
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                return await CreateCallbackResultAsync(false, "授权回调参数不完整");

            var authorizationState = ReadState(state);
            var emailAccount = await db
                .EmailAccounts.Include(x => x.OAuthCredential)
                .Include(x => x.SenderAccount)
                .Include(x => x.ReceivingAccount)
                .FirstOrDefaultAsync(x =>
                    x.Id == authorizationState.EmailAccountId
                    && x.UserId == authorizationState.UserId
                );
            if (emailAccount?.OAuthCredential == null)
                return await CreateCallbackResultAsync(false, "授权对应的邮箱账户不存在");

            var oauthCredential = emailAccount.OAuthCredential;
            var application = ResolveApplication(oauthCredential);
            var tokenResult = await ExchangeAuthorizationCodeAsync(
                oauthCredential.TokenEndpoint!,
                application,
                code
            );
            SaveCredentialTokens(oauthCredential, application, tokenResult);
            if (emailAccount.SenderAccount != null)
            {
                emailAccount.SenderAccount.Status = SenderAccountStatus.Valid;
                emailAccount.SenderAccount.ValidationFailureReason = null;
            }
            if (emailAccount.ReceivingAccount != null)
            {
                emailAccount.ReceivingAccount.Status = ReceivingAccountStatus.Active;
                emailAccount.ReceivingAccount.LastConnectedAtUtc = DateTime.UtcNow;
                emailAccount.ReceivingAccount.LastError = null;
            }
            await db.SaveChangesAsync();
            return await CreateCallbackResultAsync(true, "Microsoft Graph 授权成功");
        }
        catch (Exception exception)
        {
            Logger.Warn("Microsoft Graph 授权回调失败", exception);
            return await CreateCallbackResultAsync(false, exception.Message);
        }
    }

    private async Task<EmailAccountOAuthCredential> EnsureMicrosoftCredentialAsync(
        EmailAccount emailAccount
    )
    {
        if (emailAccount.OAuthCredential != null)
        {
            if (emailAccount.OAuthCredential.Provider != OAuthProvider.Microsoft)
                throw new KnownException("该邮箱已配置其它 OAuth 提供商");
            return emailAccount.OAuthCredential;
        }

        var credential = new EmailAccountOAuthCredential
        {
            EmailAccountId = emailAccount.Id,
            Provider = OAuthProvider.Microsoft,
            ApplicationSource = OAuthApplicationSource.System,
            TenantId = "common",
            AuthorizedScopes = string.Join(' ', AuthorizationScopes),
            TokenEndpoint = GetTokenEndpoint("common"),
            EncryptionKeyVersion = credentialProtector.ActiveKeyVersion,
            CredentialUpdatedAtUtc = DateTime.UtcNow,
        };
        db.EmailAccountOAuthCredentials.Add(credential);
        await db.SaveChangesAsync();
        emailAccount.OAuthCredential = credential;
        return credential;
    }

    private MicrosoftApplication ResolveApplication(EmailAccountOAuthCredential credential)
    {
        if (credential.ApplicationSource == OAuthApplicationSource.System)
        {
            var options = new MicrosoftEntraAppOptions();
            configuration.GetRequiredSection("MicrosoftEntraApp").Bind(options);
            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new KnownException("系统 Microsoft Entra 应用未配置 ClientId");
            return new MicrosoftApplication(
                options.ClientId,
                string.IsNullOrWhiteSpace(options.TenantId) ? "common" : options.TenantId,
                string.IsNullOrWhiteSpace(options.ClientSecret) ? null : options.ClientSecret
            );
        }

        if (string.IsNullOrWhiteSpace(credential.ClientId))
            throw new KnownException("自定义 Microsoft Entra 应用未配置 ClientId");
        return new MicrosoftApplication(
            credential.ClientId,
            string.IsNullOrWhiteSpace(credential.TenantId) ? "common" : credential.TenantId,
            string.IsNullOrWhiteSpace(credential.EncryptedClientSecret)
                ? null
                : credentialProtector.Unprotect(
                    credential.EncryptedClientSecret,
                    credential.EncryptionKeyVersion
                )
        );
    }

    private AuthorizationState ReadState(string protectedState)
    {
        var json = _stateProtector.Unprotect(protectedState);
        var state =
            JsonSerializer.Deserialize<AuthorizationState>(json)
            ?? throw new AuthenticationException("授权状态无效");
        if (state.ExpiresAtUtc < DateTime.UtcNow)
            throw new AuthenticationException("授权状态已过期，请重新发起授权");
        return state;
    }

    private async Task<AuthenticationResult2> ExchangeAuthorizationCodeAsync(
        string tokenEndpoint,
        MicrosoftApplication application,
        string code
    )
    {
        Dictionary<string, string> form =
            new()
            {
                ["client_id"] = application.ClientId,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = GetRedirectUri(),
                ["scope"] = string.Join(' ', AuthorizationScopes),
            };
        if (!string.IsNullOrWhiteSpace(application.ClientSecret))
            form["client_secret"] = application.ClientSecret;

        using var response = await httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(form)
        );
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            string? message = null;
            try
            {
                message = System
                    .Text.Json.Nodes.JsonNode.Parse(responseContent)
                    ?["error_description"]?.GetValue<string>();
            }
            catch { }
            throw new AuthenticationException(message ?? "Microsoft Graph 令牌交换失败");
        }

        var result = responseContent.JsonTo<AuthenticationResult2>();
        if (
            result == null
            || string.IsNullOrWhiteSpace(result.AccessToken)
            || string.IsNullOrWhiteSpace(result.RefreshToken)
        )
            throw new AuthenticationException("Microsoft Graph 令牌响应不完整");
        return result;
    }

    private void SaveCredentialTokens(
        EmailAccountOAuthCredential credential,
        MicrosoftApplication application,
        AuthenticationResult2 result
    )
    {
        var protectedAccessToken = credentialProtector.Protect(result.AccessToken);
        credential.EncryptedAccessToken = protectedAccessToken.Ciphertext;
        credential.EncryptedRefreshToken = credentialProtector
            .Protect(result.RefreshToken)
            .Ciphertext;
        credential.EncryptionKeyVersion = protectedAccessToken.KeyVersion;
        credential.AccessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
        credential.AuthorizedScopes = result.Scope;
        credential.TokenEndpoint = GetTokenEndpoint(application.TenantId);
        credential.CredentialUpdatedAtUtc = DateTime.UtcNow;
        if (
            credential.ApplicationSource == OAuthApplicationSource.Custom
            && !string.IsNullOrWhiteSpace(application.ClientSecret)
        )
            credential.EncryptedClientSecret = credentialProtector
                .Protect(application.ClientSecret)
                .Ciphertext;
        else
            credential.EncryptedClientSecret = null;
    }

    private async Task<ContentResult> CreateCallbackResultAsync(bool isSuccess, string message)
    {
        if (string.IsNullOrEmpty(_callbackPage))
        {
            var assemblyDirectory =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? throw new InvalidOperationException("无法确定程序集目录");
            _callbackPage = await System.IO.File.ReadAllTextAsync(
                Path.Combine(assemblyDirectory, "data/init/outlookAuthorizeCallback.html")
            );
        }

        var resultJson = JsonSerializer.Serialize(new { ok = isSuccess, message });
        return Content(
            _callbackPage.Replace("{{ authorizeResult }}", resultJson),
            "text/html; charset=utf-8"
        );
    }

    private string GetRedirectUri()
    {
        var baseUrl = configuration.GetValue<string>("BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new KnownException("BaseUrl 配置不能为空");
        return $"{baseUrl.TrimEnd('/')}/api/v1/outlook-authorization/code";
    }

    private static string GetTokenEndpoint(string tenantId) =>
        $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/token";

    private static string BuildUri(string baseUri, IReadOnlyDictionary<string, string?> query)
    {
        var queryString = string.Join(
            "&",
            query
                .Where(x => x.Value != null)
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}")
        );
        return $"{baseUri}?{queryString}";
    }

    private sealed record AuthorizationState(
        long EmailAccountId,
        long UserId,
        DateTime ExpiresAtUtc
    );

    private sealed record MicrosoftApplication(
        string ClientId,
        string TenantId,
        string? ClientSecret
    );
}
