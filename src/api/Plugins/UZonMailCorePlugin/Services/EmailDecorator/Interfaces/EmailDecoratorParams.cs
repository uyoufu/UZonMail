using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    public class EmailDecoratorParams(
        SendingSetting sendingSetting,
        SendingItem sendingItem,
        SendingItemExcelData? variables,
        SenderAccount senderAccount,
        string subject,
        string htmlBody
    ) : IContentDecoratorParams
    {
        /// <summary>
        /// 发件设置
        /// </summary>
        public SendingSetting SendingSetting { get; } = sendingSetting;

        /// <summary>
        /// 发送项
        /// </summary>
        public SendingItem SendingItem { get; } = sendingItem;

        /// <summary>
        /// 邮件变量数据
        /// </summary>
        public SendingItemExcelData? Variables { get; } = variables;

        /// <summary>
        /// 发件箱
        /// </summary>
        public SenderAccount SenderAccount { get; } = senderAccount;

        public string SenderEmail { get; } = senderAccount.Email;

        public IReadOnlyList<EmailAddress> Recipients { get; } = sendingItem.Recipients;

        public IReadOnlyList<EmailAddress> CC { get; } = sendingItem.CC ?? [];

        public IReadOnlyList<EmailAddress> BCC { get; } = sendingItem.BCC ?? [];

        public string Subject { get; } = subject;

        public string HtmlBody { get; } = htmlBody;
    }
}
