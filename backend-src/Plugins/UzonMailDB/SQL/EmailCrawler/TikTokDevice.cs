using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    public class TikTokDevice : UserAndOrgId
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public string DeviceId { get; set; }

        public string OdinId { get; set; }

        /// <summary>
        /// 是否共享
        /// 为 true 时，要同时设置 organizationId
        /// </summary>
        public bool IsShared { get; set; }
    }
}
