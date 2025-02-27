using Newtonsoft.Json;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EmailCrawler
{
    /// <summary>
    /// tiktok 作者信息
    /// </summary>
    public class TiktokAuthor : UserAndOrgId
    {
        public string AvatarLarger { get; set; }
        public string AvatarMedium { get; set; }
        public string AvatarThumb { get; set; }
        public int CommentSetting { get; set; }
        public int DownloadSetting { get; set; }
        public int DueSetting { get; set; }
        public bool Ftc { get; set; }
        public bool IsAdVirtual { get; set; }
        public bool IsEmbedBanned { get; set; }
        public string Nickname { get; set; }
        public bool OpenFavorite { get; set; }
        public bool PrivateAccount { get; set; }
        public int Relation { get; set; }
        public string SecUid { get; set; }
        public bool Secret { get; set; }
        public string? Signature { get; set; }
        public int StitchSetting { get; set; }
        public bool TtSeller { get; set; }
        public string UniqueId { get; set; }
        public bool Verified { get; set; }

        /**
         * 统计相关信息
         */
        public long DiggCount { get; set; }
        public long FollwerCount { get; set; }
        public long FollwingCount { get; set; }
        public long FreindCount { get; set; }
        public long Heart { get; set; }
        public long HeartCount { get; set; }
        public long VideoCount { get; set; }

        /**
         * 解析后的数据
         */

        /// <summary>
        /// 是否解析到了额外信息
        /// </summary>
        public bool IsParsed { get; set; }
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// WhatsApp 号码
        /// </summary>
        public string? WhatsApp { get; set; }
        /// <summary>
        /// Instagram 账号
        /// </summary>
        public string? Instagram { get; set; }
        /// <summary>
        /// Youtube 频道
        /// </summary>
        public string? Youtube { get; set; }
        /// <summary>
        /// Telegram 信息
        /// </summary>
        public string? Telegram { get; set; }

        /// <summary>
        /// 跟随作者 id
        /// </summary>
        public long FollowingAuthorId { get; set; }

        /// <summary>
        /// 已经爬取的数量
        /// </summary>
        public long CrawledCount { get; set; }
    }
}
