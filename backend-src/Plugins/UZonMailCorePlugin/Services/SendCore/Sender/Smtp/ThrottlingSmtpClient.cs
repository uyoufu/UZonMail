using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UZonMail.CorePlugin.Services.Config;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// 具有发件速率限制的 smtp 客户端
    /// 代理与客户端是绑定的
    /// </summary>
    public class ThrottlingSmtpClient(DebugConfig debugConfig) : SmtpClient, IEmailSendingClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThrottlingSmtpClient));

        private string _email;
        private int _cooldownMilliseconds;

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="email"></param>
        /// <param name="cooldownMilliseconds"></param>
        public void SetParams(string email, int cooldownMilliseconds)
        {
            _email = email.Trim();
            _cooldownMilliseconds = cooldownMilliseconds;
        }

        /// <summary>
        /// 发送的数量
        /// </summary>
        public int SentCount { get; private set; } = 0;

        /// <summary>
        /// 系统默认的设置，目前不做限制
        /// 至少间隔时间
        /// </summary>
        private static readonly int _minTimeIntervalMilliseconds = 0;

        private DateTime _lastDate = DateTime.UtcNow.AddDays(-10);

        private static SecureSocketOptions GuessSecureSocketOptions(int port, bool enableSSL)
        {
            if (port == 587)
                return SecureSocketOptions.StartTls;
            return enableSSL ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.Auto;
        }

        /// <summary>
        /// 发送邮件接口
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(MimeMessage message)
        {
            return await SendAsync(message, default, null);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public override async Task<string> SendAsync(
            MimeMessage message,
            CancellationToken cancellationToken,
            ITransferProgress progress = null
        )
        {
            var now = DateTime.UtcNow;
            var timeInverval = (int)(now - _lastDate).TotalMilliseconds;
            _lastDate = now;
            int controlValue = Math.Max(_cooldownMilliseconds, _minTimeIntervalMilliseconds);

            if (timeInverval <= controlValue)
            {
                var delayMs = controlValue - timeInverval;
                _logger.Warn($"{_email} 发件间隔太短，将在 {delayMs} 毫秒后开始发送");
                await Task.Delay(delayMs);
            }

            string? sentMessage;
            if (debugConfig.PreventSending)
            {
                sentMessage = "调试模式中已阻止真实发件";
            }
            else
            {
                sentMessage = await base.SendAsync(message, cancellationToken, progress);
            }

            // 增加本身计数
            SentCount++;

            return sentMessage;
        }

        public SmtpClientKey GetClientKey()
        {
            if (ProxyClient == null)
                return new SmtpClientKey(_email, string.Empty);

            // 如果存在代理，则返回包含代理的 host 的 key
            if (ProxyClient is not ProxyClientAdapter proxy)
            {
                throw new TypeAccessException("代理客户端类型错误, 只能是 ProxyClientAdapter 或其子类");
            }

            return new SmtpClientKey(_email, proxy.ProxyHost);
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
    }
}
