using Innofactor.EfCoreJsonValueConverter;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Core.Organization;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 邮箱组
    /// 为了方便搜索和减少复杂度，使用扁平化的组，不使用树形结构
    /// </summary>
    public class EmailGroup : SqlId
    {
        public long UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// 类型
        /// 1-发件箱
        /// 2-收件箱
        /// </summary>
        public EmailGroupType Type { get; set; }

        /// <summary>
        /// 图标名称
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// 父组 id
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public long Order { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<Inbox> Inboxes { get; set; }

        /// <summary>
        /// 是否是默认分组
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 默认的分组名
        /// </summary>
        public static string DefaultGroupName => "未分组";

        /// <summary>
        /// 额外的数据
        /// 比如关联qq组号
        /// 允许为空
        /// </summary>
        public string? Extra { get; set; }

        /// <summary>
        /// 获取默认的邮箱组
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="emailGroupType"></param>
        /// <returns></returns>
        public static EmailGroup GetDefaultEmailGroup(long userId, EmailGroupType emailGroupType)
        {
            return new EmailGroup()
            {
                IsDefault = true,
                Name = DefaultGroupName,
                Description = "默认邮箱组",
                Order = 0,
                Type = emailGroupType,
                UserId = userId,
            };
        }
    }
}
