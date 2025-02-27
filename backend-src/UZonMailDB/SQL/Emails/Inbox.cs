using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.DB.SQL.Emails
{
    /// <summary>
    /// 收件箱
    /// </summary>
    public class Inbox : EmailBox
    {
        /// <summary>
        /// 收件箱所属的组织
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        /// 上一次成功发件的日期
        /// 同一个组织内，共用日期
        /// </summary>
        public DateTime LastSuccessDeliveryDate { get; set; }

        /// <summary>
        /// 上一次被发件日期
        /// </summary>
        public DateTime LastBeDeliveredDate { get; set; }

        /// <summary>
        /// 最短收件间隔时间，单位小时
        /// 负数表示不限制
        /// </summary>
        public long MinInboxCooldownHours { get; set; } = -1;
    }
}
