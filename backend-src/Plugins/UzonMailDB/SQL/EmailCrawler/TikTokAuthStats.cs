using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class TikTokAuthStats : SqlId
    {
        public long TikTokAuthorId { get; set; }
        public TiktokAuthor TiktokAuthor { get; set; }

        public int DiggCount { get; set; }
        public int FollwerCount { get; set; }
        public int FollwingCount { get; set; }
        public int FreindCount { get; set; }
        public int Heart { get; set; }
        public int HeartCount { get; set; }
        public int VideoCount { get; set; }
    }
}
