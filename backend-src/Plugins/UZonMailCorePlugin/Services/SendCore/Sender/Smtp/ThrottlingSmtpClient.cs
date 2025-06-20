using log4net;
using MailKit;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Sockets;
using Uamazing.Utils.Envs;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// 具有发件速率限制的 smtp 客户端
    /// 代理与客户端是绑定的
    /// </summary>
    public class ThrottlingSmtpClient(string email, int cooldownMilliseconds) : SmtpClient, IAuthenticateClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThrottlingSmtpClient));

        /// <summary>
        /// 发送的数量
        /// </summary>
        public int SentCount { get; private set; } = 0;

        /// <summary>
        /// 系统默认的设置，目前不做限制
        /// 至少间隔时间
        /// </summary>
        private static readonly int _minTimeIntervalMilliseconds = 0;

        private DateTime _lastDate = DateTime.Now.AddDays(-10);

        public SecureSocketOptions GuessSecureSocketOptions(int port, bool enableSSL)
        {
            if (port == 587) return SecureSocketOptions.StartTls;
            return enableSSL ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.Auto;
        }



        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public override async Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            var now = DateTime.Now;
            var timeInverval = (int)(now - _lastDate).TotalMilliseconds;
            _lastDate = now;
            int controlValue = Math.Max(cooldownMilliseconds, _minTimeIntervalMilliseconds);

            if (timeInverval <= controlValue)
            {
                var delayMs = controlValue - timeInverval;
                _logger.Warn($"{email} 发件间隔太短，将在 {delayMs} 毫秒后开始发送");
                await Task.Delay(delayMs);
            }

            var sentMessage = await base.SendAsync(message, cancellationToken, progress);

            // 增加本身计数
            SentCount++;

            return sentMessage;
        }

        public SmtpClientKey GetClientKey()
        {
            if (ProxyClient == null) return new SmtpClientKey(email, string.Empty);

            // 如果存在代理，则返回包含代理的 host 的 key
            if (ProxyClient is not ProxyClientAdapter proxy)
            {
                throw new TypeAccessException("代理客户端类型错误, 只能是 ProxyClientAdapter 或其子类");
            }

            return new SmtpClientKey(email, proxy.ProxyHost);
        }

        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task AuthenticateAsync(string email, string username, string password)
        {
            await base.AuthenticateAsync(username, password);
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
            // 判断参数是否正确
            if (string.IsNullOrEmpty(smtpHost))
            {
                return Result<string>.Fail("SMTP 服务器地址不能为空");
            }
            if (smtpPort <= 0 || smtpPort > 65535)
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
            using var client = new ThrottlingSmtpClient(email, 0);
            client.ProxyClient = proxyClient;

            string sendResult = $"{smtpUserName} test success";
            try
            {
                await client.ConnectAsync(smtpHost, smtpPort, enableSSL);
                // 鉴权
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(email, smtpUserName, smtpPassword);
                }
                return Result<string>.Success(sendResult);
            }
            catch (SslHandshakeException ex)
            {
                _logger.Warn(ex);
                // 可能是证书过期
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
                // 鉴权
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(email, smtpUserName, smtpPassword);
                }
                return Result<string>.Success(sendResult);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                return Result<string>.Fail(ex.Message);
            }
            finally
            {
                // 断开连接
                await client.DisconnectAsync(true);
            }
        }
    }
}
