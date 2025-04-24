using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Uamazing.Utils.Envs;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;

namespace UZonMail.Core.Services.SendCore.Sender
{
    /// <summary>
    /// 具有发件速率限制的 smtp 客户端
    /// 代理与客户端是绑定的
    /// </summary>
    public class ThrottlingSmtpClient(string email, int cooldownMilliseconds) : SmtpClient
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
            if(ProxyClient==null)return new SmtpClientKey(email, string.Empty);

            // 如果存在代理，则返回包含代理的 host 的 key
            if(ProxyClient is not ProxyClientAdapter proxy)
            {
                throw new TypeAccessException("代理客户端类型错误, 只能是 ProxyClientAdapter 或其子类");
            }

            return new SmtpClientKey(email, proxy.ProxyHost);
        }
    }
}
