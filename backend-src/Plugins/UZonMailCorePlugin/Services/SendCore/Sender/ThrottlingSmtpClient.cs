using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Uamazing.Utils.Envs;

namespace UZonMail.Core.Services.SendCore.Sender
{
    /// <summary>
    /// 具有发件速率限制的 smtp 客户端
    /// </summary>
    public class ThrottlingSmtpClient(string email, int cooldownMilliseconds) : SmtpClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThrottlingSmtpClient));

        /// <summary>
        /// 至少间隔时间
        /// </summary>
        private static readonly int _minTimeIntervalMilliseconds = 500;

        private DateTime _lastDate = DateTime.Now.AddDays(-1);

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
            int controlValue = _minTimeIntervalMilliseconds;
            if (cooldownMilliseconds > 0)
                controlValue = Math.Max(cooldownMilliseconds, _minTimeIntervalMilliseconds);

            if (timeInverval <= controlValue)
            {
                _logger.Warn($"{email} 发件间隔太短，将在 {timeInverval} 毫秒后开始发送");
                await Task.Delay(timeInverval);
            }

            var sentMessage = "send by debug";
            if (!Env.IsDebug)
                sentMessage = await base.SendAsync(message, cancellationToken, progress);
            return sentMessage;
        }
    }
}
