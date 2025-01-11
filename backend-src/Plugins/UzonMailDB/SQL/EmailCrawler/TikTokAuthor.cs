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
        /// <summary>
        /// tiktok id 的字符串形式
        /// </summary>
        public string AuthorId { get; set; }
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

        public string? Email { get; set; }
    }
}
