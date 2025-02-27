using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Unsubscribes
{
    /// <summary>
    /// 退订邮件
    /// </summary>
    [Index(nameof(OrganizationId), nameof(Email))]
    public class UnsubscribeEmail : OrgId
    {
        /// <summary>
        /// 退订时的 IP 地址
        /// 方便定位退订的来源
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// 退订邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
