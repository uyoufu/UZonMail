using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class TikTokDevice : UserAndOrgId
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public long DeviceId { get; set; }

        public long OdinId { get; set; }
    }
}
