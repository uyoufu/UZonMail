using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 发件项与收件联系人对应的表
    /// 可以通过该表查询最新的发件时间和收件时间
    /// </summary>
    public class SendingItemRecipient : OrgId
    {
        public long SendingItemId { get; set; }

        public long RecipientContactId { get; set; }

        /// <summary>
        /// 收件邮箱
        /// </summary>
        public string? RecipientEmail { get; set; }

        /// <summary>
        /// 实际发件地址
        /// </summary>
        public string? SenderEmail { get; set; }

        /// <summary>
        /// 标识角色
        /// </summary>
        public RecipientRole Role { get; set; }

        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime SendDate { get; set; }
    }

    public enum RecipientRole
    {
        /// <summary>
        /// 收件人
        /// </summary>
        Recipient,

        /// <summary>
        /// 抄送人
        /// </summary>
        CC,

        /// <summary>
        /// 密送人
        /// </summary>
        BCC
    }
}
