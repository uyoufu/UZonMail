using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class TikTokAuthStats
    {
        public int DiggCount { get; set; }
        public int FollwerCount { get; set; }
        public int FollwingCount { get; set; }
        public int FreindCount { get; set; }
        public int Heart { get; set; }
        public int HeartCount { get; set; }
        public int VideoCount { get; set; }

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
