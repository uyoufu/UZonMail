using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Files
{
    /// <summary>
    /// 经过内容寻址后保存在磁盘中的物理文件。
    /// </summary>
    public class FileObject : SqlId, IEntityTypeConfiguration<FileObject>
    {
        public long FileBucketId { get; set; }
        public FileBucket FileBucket { get; set; } = null!;

        public DateTime LastModifyDate { get; set; }

        public string Sha256 { get; set; } = string.Empty;

        /// <summary>
        /// 相对于存储桶根目录的路径。
        /// </summary>
        public string Path { get; set; } = string.Empty;

        public long Size { get; set; }

        public FileObjectStorageState StorageState { get; set; }

        /// <summary>
        /// 配置物理文件的唯一性约束。
        /// </summary>
        public void Configure(EntityTypeBuilder<FileObject> builder)
        {
            builder.HasIndex(x => x.Sha256).IsUnique();
        }
    }
}
