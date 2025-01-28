using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class TikTokAuthStats
    {
        public long DiggCount { get; set; }
        public long FollwerCount { get; set; }
        public long FollwingCount { get; set; }
        public long FreindCount { get; set; }
        public long Heart { get; set; }
        public long HeartCount { get; set; }
        public long VideoCount { get; set; }

        public void SetTo(TiktokAuthor author)
        {
            author.DiggCount = DiggCount;
            author.FollwerCount = FollwerCount;
            author.FollwingCount = FollwingCount;
            author.FreindCount = FreindCount;
            author.Heart = Heart;
            author.HeartCount = HeartCount;
            author.VideoCount = VideoCount;
        }
    }
}
