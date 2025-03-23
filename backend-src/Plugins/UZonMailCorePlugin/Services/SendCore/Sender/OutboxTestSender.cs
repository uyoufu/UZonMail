using log4net;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.DB.SQL;
using UZonMail.Utils.Results;
using UZonMail.Utils.Extensions;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Core.Services.Config;

namespace UZonMail.Core.Services.SendCore.Sender
{
    /// <summary>
    /// 发件箱测试
    /// </summary>
    /// <param name="outbox"></param>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="outbox"></param>
    /// <param name="smtpPasswordSecretKeys"></param>
    /// <param name="sqlContext"></param>
    public class OutboxTestSender(Outbox outbox, SmtpPasswordSecretKeys smtpPasswordSecretKeys, SqlContext sqlContext, DebugConfig debugConfig)
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxTestSender));

        /// <summary>
        /// 测试不需要实际进行发件
        /// [TODO]: 需要进行性能优化：1-同类型的验证进行复用 2-增加代理
        /// </summary>
        /// <returns></returns>
        public async Task<Result<string>> SendTest()
        {
            // 参考：https://github.com/jstedfast/MailKit/tree/master/Documentation/Examples
            // 本机发件逻辑
            var message = new MimeMessage();
            var mailbox = new MailboxAddress(outbox.Name, outbox.Email);
            // 发件人
            message.From.Add(mailbox);
            // 主题
            message.Subject = "SMTP Testing";
            message.To.Add(mailbox);
            // 正文
            BodyBuilder bodyBuilder = new()
            {
                HtmlBody = "This email is for SMTP Testing from UZonMail"
            };
            message.Body = bodyBuilder.ToMessageBody();
            try
            {
                using var client = new SmtpClient();
                // 获取代理
                if (outbox.ProxyId > 0)
                {
                    // 获取当前用户信息
                    var user = await sqlContext.Users.AsNoTracking().Where(x => x.Id == outbox.UserId).FirstOrDefaultAsync();
                    var proxy = await sqlContext.Proxies.Where(x => x.OrganizationId == user.OrganizationId)
                        .Where(x => x.Id == outbox.ProxyId)
                        .FirstOrDefaultAsync();
                    if (proxy != null)
                    {
                        // 从代理管理器中获取代理

                        client.ProxyClient = proxy.ToProxyInfo()?.GetProxyClient(_logger);
                    }
                }
                client.Connect(outbox.SmtpHost, outbox.SmtpPort, outbox.EnableSSL);
                // 鉴权
                if (!string.IsNullOrEmpty(outbox.Password))
                {
                    var password = outbox.Password.DeAES(smtpPasswordSecretKeys.Key, smtpPasswordSecretKeys.Iv);
                    client.Authenticate(string.IsNullOrEmpty(outbox.UserName) ? outbox.Email : outbox.UserName, password);
                }
                string sendResult = "fake sending by debug";

                // 测试发件，不需要真实发送，只需要验证授权即可
                //if (!debugConfig.IsDemo)
                //    sendResult = await client.SendAsync(message);

                return new Result<string>(true, sendResult);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                return new Result<string>(false, ex.Message, ex.Message);
            }
        }
    }
}
