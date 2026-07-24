using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender
{
    /// <summary>
    /// 邮件发送管理器
    /// </summary>
    /// <param name="emailSenders"></param>
    public class EmailSendersManager(IEnumerable<IEmailTransport> transports) : ISingletonService
    {
        /// <summary>
        /// 获取邮件发送器
        /// </summary>
        /// <param name="outboxEmail"></param>
        /// <returns></returns>
        public IEmailTransport GetEmailSender(OutboxType outboxType)
        {
            var matches = transports.Where(x => x.Type == outboxType).ToList();
            return matches.Count switch
            {
                1 => matches[0],
                0
                    => throw new InvalidOperationException(
                        $"未找到匹配的邮件 Transport，OutboxType：{outboxType}"
                    ),
                _
                    => throw new InvalidOperationException(
                        $"找到多个邮件 Transport，OutboxType：{outboxType}"
                    ),
            };
        }
    }
}
