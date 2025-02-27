namespace UZonMail.DB.SQL.EmailCrawler
{
    public enum CrawlerStatus
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created,

        /// <summary>
        /// 进行中
        /// </summary>
        Running,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped,
    }
}
