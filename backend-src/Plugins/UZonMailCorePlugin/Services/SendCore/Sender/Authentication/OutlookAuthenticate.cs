using log4net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Tls;
using UZonMail.Core.Services.SendCore.Outboxes;

namespace UZonMail.Core.Services.SendCore.Sender.Authentication
{
    /// <summary>
    /// Outlook 的 OAuth2 验证
    /// 参考：https://github.com/jstedfast/MailKit/blob/master/ExchangeOAuth2.md#authenticating-a-web-service-with-oauth2
    /// </summary>
    public class OutlookAuthenticate : ISmtpAuthenticate
    {
        public int Order => 0;
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutlookAuthenticate));

        public async Task AuthenticateAsync(SmtpClient smtpClient, string email, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.Warn("Outlook 邮箱没有配置用户名或密码，无法进行 OAuth2 验证。");
                return;
            }

            var ids = username.Split("/");
            if (ids.Length < 2)
            {
                _logger.Warn("Outlook 邮箱的用户名格式不正确，应为 tenantId/clientId 的形式。");
                return;
            }

            var tenantId = ids[0];
            var clientId = ids[1];

            var oauth2Credentials = await GetConfidentialClientOAuth2CredentialsAsync("SMTP", tenantId, clientId, password);
            var oauth2 = new SaslMechanismOAuth2(email, oauth2Credentials.AccessToken);
            await smtpClient.AuthenticateAsync(oauth2);
        }

        public bool Match(string outbox)
        {
            return outbox.Contains("@outlook.com", StringComparison.CurrentCultureIgnoreCase);
        }

        private static async Task<AuthenticationResult> GetConfidentialClientOAuth2CredentialsAsync(string protocol, string tenantId, string clientId, string clientSecret)
        {
            var loginUrl = "https://login.chinacloudapi.cn/";
            var loginUrl2 = "https://login.microsoftonline.com/";
            var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithAuthority($"{loginUrl2}{tenantId}/v2.0")                
                .WithClientSecret(clientSecret) // or .WithClientSecret (clientSecret)
                .Build();

            string[] scopes;

            if (protocol.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
            {
                scopes = [
                // For SMTP, use the following scope
                "https://outlook.office365.com/.default"
                ];
            }
            else
            {
                scopes = [
                    // For IMAP and POP3, use the following scope
                    "https://ps.outlook.com/.default"
                ];
            }

            return await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();
        }
    }
}
