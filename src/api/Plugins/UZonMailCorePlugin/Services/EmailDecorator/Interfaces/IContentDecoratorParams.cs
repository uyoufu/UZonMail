using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    public interface IContentDecoratorParams
    {
        /// <summary>
        /// 发件设置
        /// </summary>
        SendingSetting SendingSetting { get; }

        /// <summary>
        /// 发送项
        /// </summary>
        SendingItem SendingItem { get; }

        /// <summary>
        /// 邮件变量数据
        /// </summary>
        SendingItemExcelData? Variables { get; }

        /// <summary>
        /// 发件箱
        /// </summary>
        SenderAccount SenderAccount { get; }

        /// <summary>
        /// 发件箱邮箱
        /// </summary>
        string SenderEmail { get; }

        IReadOnlyList<EmailAddress> Recipients { get; }

        IReadOnlyList<EmailAddress> CC { get; }

        IReadOnlyList<EmailAddress> BCC { get; }

        string Subject { get; }

        string HtmlBody { get; }
    }
}
