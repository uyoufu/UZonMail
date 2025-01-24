using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class CrawlerTaskResult : SqlId
    {
        /// <summary>
        /// 任务 id
        /// </summary>
        public long CrawlerTaskInfoId { get; set; }

        /// <summary>
        /// 用户 id
        /// </summary>
        public long TikTokAuthorId { get; set; }

        /// <summary>
        /// 是否存在额外的信息
        /// </summary>
        public bool ExistExtraInfo { get; set; }
    }
}
