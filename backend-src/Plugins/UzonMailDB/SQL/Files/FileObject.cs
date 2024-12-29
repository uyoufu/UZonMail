using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Files
{
    /// <summary>
    /// 文件类
    /// </summary>
    public class FileObject:SqlId
    {
        public long FileBucketId { get; set; }
        public FileBucket FileBucket { get; set; }

        /// <summary>
        /// 最后修改日期
        /// </summary>
        public DateTime LastModifyDate { get; set; }

        /// <summary>
        /// 哈希值
        /// </summary>
        public string Sha256 { get; set; }

        /// <summary>
        /// 文件位置
        /// 相对于桶根目录的路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 引用数量
        /// 当为负数时，表示可以删除
        /// </summary>
        public int LinkCount { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
    }
}
