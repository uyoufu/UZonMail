using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Files
{
    /// <summary>
    /// 文件存储桶
    /// </summary>
    public class FileBucket : SqlId
    {
        /// <summary>
        /// 桶名称
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 存放根目录
        /// </summary>
        public string RootDir { get; set; }

        /// <summary>
        /// 是否是默认桶
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
