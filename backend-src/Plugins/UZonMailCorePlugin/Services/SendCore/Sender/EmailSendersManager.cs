using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Sender
{
    /// <summary>
    /// 邮件发送管理器
    /// </summary>
    /// <param name="emailSenders"></param>
    public class EmailSendersManager(IEnumerable<IEmailSender> emailSenders) : ISingletonService
    {
        /// <summary>
        /// 获取邮件发送器
        /// </summary>
        /// <param name="outboxEmail"></param>
        /// <returns></returns>
        public IEmailSender GetEmailSender(OutboxType outboxType)
        {
            // 调用发件器进行发件
            var sender =
                emailSenders
                    .Where(x => x.IsMatch(outboxType))
                    .OrderBy(x => x.Order)
                    .FirstOrDefault()
                ?? throw new InvalidOperationException($"未找到匹配的邮件发送器，OutboxType：{outboxType}");

            return sender;
        }
    }
}
