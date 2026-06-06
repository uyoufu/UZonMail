using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.EmailDecorator.Interfaces
{
    public class EmailDecoratorParams(
        SendingSetting sendingSetting,
        SendItemMeta sendItemMeta,
        Outbox outbox
    ) : IContentDecoratorParams
    {
        /// <summary>
        /// 发件设置
        /// </summary>
        public SendingSetting SendingSetting { get; } = sendingSetting;

        /// <summary>
        /// 发送项
        /// </summary>
        public SendingItem SendingItem { get; } = sendItemMeta.SendingItem;

        /// <summary>
        /// 发件项在运行中产生的数据
        /// </summary>
        public SendItemMeta SendItemMeta { get; } = sendItemMeta;

        /// <summary>
        /// 发件箱
        /// </summary>
        public Outbox Outbox { get; set; } = outbox;

        public string OutboxEmail { get; set; } = outbox.Email;
    }
}
