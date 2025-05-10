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
using UZonMail.DB.MySql;
using MailKit.Net.Proxy;
using UZonMail.Core.Utils.FluentMailkit;
using MailKit.Security;

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
    public class OutboxTestSender(SqlContext sqlContext)
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxTestSender));

        /// <summary>
        /// 测试发件箱
        /// 不会真实发件，只会进行连接测试
        /// </summary>
        /// <returns></returns>
        public async Task<Result<string>> SendTest(Outbox outbox, SmtpPasswordSecretKeys smtpPasswordSecretKeys)
        {
            // 解析密码
            string smtpPassword = string.Empty;
            if (!string.IsNullOrEmpty(outbox.Password))
                smtpPassword = outbox.Password.DeAES(smtpPasswordSecretKeys.Key, smtpPasswordSecretKeys.Iv);

            ProxyClient? proxyClient = null;
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
                    proxyClient = proxy.ToProxyInfo()?.GetProxyClient(_logger);
                }
            }

            var smtpUsername = string.IsNullOrEmpty(outbox.UserName) ? outbox.Email : outbox.UserName;
            return  SendTest(outbox.SmtpHost, outbox.SmtpPort, outbox.EnableSSL, smtpUsername, smtpPassword, proxyClient);
        }

        /// <summary>
        /// 测试发件箱
        /// TODO: 需要进行性能优化：1-同类型的验证进行复用
        /// </summary>
        /// <param name="outboxName"></param>
        /// <param name="outboxEmail"></param>
        /// <param name="smtpHost"></param>
        /// <param name="smtpPort"></param>
        /// <param name="enableSSL"></param>
        /// <param name="smtpUserName"></param>
        /// <param name="smtpPassword"></param>
        /// <param name="proxyClient"></param>
        /// <returns></returns>
        public Result<string> SendTest(string smtpHost, int smtpPort, bool enableSSL,
            string smtpUserName, string smtpPassword, IProxyClient? proxyClient = null)
        {
            // 判断参数是否正确
            if (string.IsNullOrEmpty(smtpHost))
            {
                return Result<string>.Fail("SMTP 服务器地址不能为空");
            }
            if (smtpPort <= 0 || smtpPort> 65535)
            {
                return Result<string>.Fail("SMTP 端口号不正确");
            }
            if (string.IsNullOrEmpty(smtpUserName))
            {
                return Result<string>.Fail("SMTP 用户名不能为空");
            }
            if (string.IsNullOrEmpty(smtpPassword))
            {
                return Result<string>.Fail("SMTP 密码不能为空");
            }

            // 参考：https://github.com/jstedfast/MailKit/tree/master/Documentation/Examples
            using var client = new SmtpClient();
            client.ProxyClient = proxyClient;

            string sendResult = $"{smtpUserName} test success";
            try
            {
                client.Connect(smtpHost, smtpPort, enableSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);
                // 鉴权
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    client.Authenticate(smtpUserName, smtpPassword);
                }                
                return Result<string>.Success(sendResult);
            }
            catch (SslHandshakeException ex)
            {
                _logger.Warn(ex);
                // 可能是证书过期
                client.Connect(smtpHost, smtpPort, SecureSocketOptions.None);
                // 鉴权
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    client.Authenticate(smtpUserName, smtpPassword);
                }
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
