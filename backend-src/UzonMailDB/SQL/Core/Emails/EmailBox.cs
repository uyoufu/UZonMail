using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// EmailBox 基类
    /// </summary>
    [Index(nameof(UserId))]
    [Index(nameof(Email))]
    public class EmailBox : SqlId
    {
        /// <summary>
        /// 邮件组 Id
        /// </summary>
        public long EmailGroupId { get; set; }

        // 冗余用户信息
        public long UserId { get; set; }

        private string _email = string.Empty;

        /// <summary>
        /// 收件箱
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                // 去掉 email 两端空格
                _email = value.Trim();
                var index = _email.LastIndexOf('@');
                // 提取邮箱类型
                if (index > 0)
                {
                    Domain = _email[(index + 1)..];
                }
            }
        }

        /// <summary>
        /// 邮箱域名
        /// 方便统计，赋予 Email 时自动赋予
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 关联数
        /// 表示该邮件用于发件或收件的数量
        /// </summary>
        public int LinkCount { get; set; }
    }
}
