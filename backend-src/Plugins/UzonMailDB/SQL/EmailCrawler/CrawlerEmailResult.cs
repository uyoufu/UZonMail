using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    /// <summary>
    /// 爬取结果
    /// </summary>
    public class CrawlerEmailResult : SqlId
    {
        /// <summary>
        /// 爬虫任务 id
        /// </summary>
        public long CrawlerTaskId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// TikTok 作者 Id
        /// 若有，则大于 0
        /// </summary>
        public int TikTokAuthorId { get; set; }
    }
}
