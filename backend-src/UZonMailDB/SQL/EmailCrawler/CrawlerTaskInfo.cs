using MailKit.Net.Proxy;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Settings;

namespace UZonMail.DB.SQL.EmailCrawler
{
    /// <summary>
    /// 邮件爬虫任务
    /// </summary>
    public class CrawlerTaskInfo : SqlId
    {
        /// <summary>
        /// 所属用户
        /// </summary>
        public long UserId { get; set; }

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
        /// 代理
        /// </summary>
        public long ProxyId { get; set; }

        /// <summary>
        /// 自动结束日期
        /// </summary>
        public DateTime Deadline { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 设备 id
        /// </summary>
        public long TikTokDeviceId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 发件箱组 id
        /// </summary>
        public long OutboxGroupId { get; set; }
    }
}
