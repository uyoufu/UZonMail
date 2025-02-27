using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.DB.SQL.EmailSending
{
    /// <summary>
    /// 发件项与发件箱对应的表
    /// </summary>
    public class SendingItemInbox : OrgId
    {
        public long SendingItemId { get; set; }
        public SendingItem SendingItem { get; set; }

        public long InboxId { get; set; }
        public Inbox Inbox { get; set; }

        /// <summary>
        /// 收件邮箱
        /// </summary>
        public string? ToEmail { get; set; }

        /// <summary>
        /// 发件箱
        /// </summary>
        public string? FromEmail { get; set; }

        /// <summary>
        /// 标识角色
        /// </summary>
        public InboxRole Role { get; set; } 

        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime SendDate { get; set; }
    }

    public enum InboxRole
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
