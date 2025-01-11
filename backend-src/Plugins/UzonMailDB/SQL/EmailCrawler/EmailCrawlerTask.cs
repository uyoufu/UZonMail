using MailKit.Net.Proxy;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Settings;

namespace UZonMail.DB.SQL.EmailCrawler
{
    /// <summary>
    /// 邮件爬虫任务
    /// </summary>
    public class EmailCrawlerTask : SqlId
    {
        /// <summary>
        /// 名称，必须是唯一的
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 爬虫类型
        /// </summary>
        public CrawlerType Type { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public CrawlerStatus Status { get; set; }

        /// <summary>
        /// 组织代理
        /// </summary>
        public OrganizationProxy OrganizationProxy { get; set; }
    }
}
