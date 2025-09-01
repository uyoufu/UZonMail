using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Sender
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
        public IEmailSender GetEmailSender(string outboxEmail, string smtpHost)
        {
            // 调用发件器进行发件
            return emailSenders.Where(x => x.IsMatch(outboxEmail, smtpHost))
                .OrderBy(x => x.Order)
                .First();
        }
    }
}
