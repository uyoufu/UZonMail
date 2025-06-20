using log4net;
using MailKit.Net.Proxy;
using MailKit.Security;
using Microsoft.Identity.Client;
using MimeKit;
using System.Net;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    /// <summary>
    /// 参考： https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp#client-credentials-provider
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cooldownMilliseconds"></param>
    public class MsGraphClient(string email, int cooldownMilliseconds) : IAuthenticateClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MsGraphClient));
        private static HttpClient _httpClient = new();

        private AuthenticationResult? _authenticationResult;
        private string _authenticateKey = string.Empty;

        /// <summary>
        /// 验证邮箱是否正确
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="AuthenticationException"></exception>
        public async Task AuthenticateAsync(string email, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                string message = "Outlook 邮箱没有配置用户名或密码，无法进行 OAuth2 验证。";
                _logger.Warn(message);
                throw new AuthenticationException(message);
            }

            var tempKey = $"{email}-{username}-{password}";
            if (_authenticationResult != null
                && _authenticationResult.ExpiresOn.LocalDateTime > DateTime.Now
                && _authenticateKey == tempKey) return;
            _authenticateKey = tempKey;

            var ids = username.Split("/");
            if (ids.Length == 0)
            {
                _logger.Warn("Outlook 邮箱应输入 ClientId");
                throw new AuthenticationException("Outlook 邮箱应输入 ClientId");
            }

            // 授权码流（Authorization Code Flow）形式
            if (ids.Length == 1)
            {
                // 说明是 clientId, refreshToken 的形式
                var codeFlowResult = await GetAccessByRefreshToken(ids[0], password)
                    ?? throw new AuthenticationException("Outlook 邮箱的 refreshToken 无效或已过期，请检查配置。");
                _authenticationResult = codeFlowResult.ToAuthenticationResult();

                return;
            }

            // 客户端凭据流（Client Credentials Flow）形式
            if (ids.Length < 2)
            {
                _logger.Warn("Outlook 邮箱的用户名格式不正确，应为 tenantId/clientId 的形式。");
                throw new AuthenticationException("Outlook 邮箱的用户名格式不正确，应为 tenantId/clientId 的形式。");
            }

            var tenantId = ids[0];
            var clientId = ids[1];

            _authenticationResult = await GetConfidentialClientOAuth2CredentialsAsync(tenantId, clientId, password);
            return;
        }

        /// <summary>
        /// 正常情况下获取 accessToken
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="tenantId"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        private static async Task<AuthenticationResult> GetConfidentialClientOAuth2CredentialsAsync(string tenantId, string clientId, string clientSecret)
        {
            var loginUrl = "https://login.microsoftonline.com/";
            var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithAuthority($"{loginUrl}{tenantId}/v2.0")
                .WithClientSecret(clientSecret) // or .WithClientSecret (clientSecret)
                .Build();


            //var scopes = [
            //      // For IMAP and POP3, use the following scope
            //      "https://ps.outlook.com/.default"
            //  ];
            // For SMTP, use the following scope
            var scopes = new List<string>() { "https://graph.microsoft.com/.default" };
            return await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();
        }

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private static async Task<AuthenticationRefreshResult?> GetAccessByRefreshToken(string clientId, string refreshToken)
        {
            var token_url = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
            var fluentHttpRequest = new FluentHttpRequest()
                .WithHttpClient(_httpClient)
                .WithMethod(HttpMethod.Post)
                .WithUrl(token_url)
                .WithFormContent(new Dictionary<string, string>()
                {
                    { "client_id", clientId },
                    { "refresh_token", refreshToken},
                    { "grant_type", "refresh_token"},
                    { "scope", "https://outlook.office365.com/.default"}
                });
            var jsonResult = await fluentHttpRequest.GetJsonAsync<AuthenticationRefreshResult>();

            // 若有新的 refresh_token, 则要保存到数据库中

            return jsonResult;
        }

        /// <summary>
        /// 开始发件
        /// 参考: https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0&tabs=http#request-3
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(MimeMessage mimeMessage)
        {
            var encodedEmail = Uri.EscapeDataString(email);

            var request = new MsGraphSendMailRequest()
                 .WithAccessToken(_authenticationResult.AccessToken)
                 .WithMimeMessage(mimeMessage)
                 .WithUrl($"https://graph.microsoft.com/v1.0/users/{encodedEmail}/sendMail")
                 .WithHttpClient(_httpClient);

            var response = await request.SendAsync();
            // 根据状态返回发送结果
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return string.Empty;
            }

            // 其它情况，表示发送失败
            _logger.Error($"发件箱 {email} 错误。{response.ReasonPhrase}");
            var responseResult = await response.Content.ReadAsStringAsync();
            _logger.Error($"发件箱 {email} 错误详情：{responseResult}");

            // 抛出异常
            throw new Exception($"发件箱 {email} 错误：{response.ReasonPhrase}，详情：{responseResult}");
        }

        /// <summary>
        /// 授权测试
        /// </summary>
        /// <param name="email"></param>
        /// <param name="smtpUserName"></param>
        /// <param name="smtpPassword"></param>
        /// <param name="proxyClient"></param>
        /// <param name="smtpHost"></param>
        /// <param name="smtpPort"></param>
        /// <param name="enableSSL"></param>
        /// <returns></returns>
        public async Task<Result<string>> AuthenticateTestAsync(string email, string smtpUserName, string smtpPassword, IProxyClient? proxyClient = null,
            string smtpHost = "", int smtpPort = 465, bool enableSSL = true)
        {
            string sendResult = $"{smtpUserName} test success";
            try
            {
                await AuthenticateAsync(email, smtpUserName, smtpPassword);
                return Result<string>.Success(sendResult);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                return Result<string>.Fail(ex.Message);
            }
        }
    }
}
