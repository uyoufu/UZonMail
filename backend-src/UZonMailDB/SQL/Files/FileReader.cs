using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Files
{
    /// <summary>
    /// 文件临时读取
    /// </summary>
    public class FileReader : SqlId
    {
        public FileReader() { }

        public FileReader(FileUsage fileUsage)
        {
            FileObjectId = fileUsage.FileObjectId;
            UserId = fileUsage.OwnerUserId;
            ExpireDate = DateTime.Now.AddMinutes(2);
            FileName = fileUsage.DisplayName ?? fileUsage.FileName;
        }

        /// <summary>
        /// 包含后缀的文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件对象的 Id
        /// </summary>
        public long FileObjectId { get; set; }

        /// <summary>
        /// 文件对象导航属性
        /// </summary>
        public FileObject FileObject { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// 用户 Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 访问次数
        /// </summary>
        public int VisitedCount { get; set; }

        /// <summary>
        /// 第一次访问日期
        /// </summary>
        public DateTime FirstDate { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// 最近一次访问日期
        /// </summary>
        public DateTime LastDate { get; set; }

        /// <summary>
        /// 最大访问次数
        /// 为 0 表示不限制
        /// </summary>
        public int MaxVisitCount { get; set; }
    }
}
