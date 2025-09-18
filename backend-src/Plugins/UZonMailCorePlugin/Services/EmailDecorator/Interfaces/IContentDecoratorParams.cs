using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.EmailDecorator.Interfaces
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
        /// 发件项在运行中产生的数据
        /// </summary>
        SendItemMeta SendItemMeta { get; }

        /// <summary>
        /// 发件箱
        /// </summary>
        Outbox Outbox { get; set; }

        /// <summary>
        /// 发件箱邮箱
        /// </summary>
        string OutboxEmail { get; set; }
    }
}
