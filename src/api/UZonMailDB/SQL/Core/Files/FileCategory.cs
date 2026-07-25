using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Organization;

namespace UzonMail.DB.SQL.Core.Files
{
    /// <summary>
    /// 用户文件的树形分类。
    /// </summary>
    public class FileCategory : SqlId, IEntityTypeConfiguration<FileCategory>
    {
        public const string DefaultName = "Default";

        public long OwnerUserId { get; set; }
        public User OwnerUser { get; set; } = null!;

        public long? ParentId { get; set; }
        public FileCategory? Parent { get; set; }

        public string Name { get; set; } = string.Empty;
        public long Sort { get; set; }
        public bool IsDefault { get; set; }

        public List<FileCategory> Children { get; set; } = [];
        public List<FileUsage> FileUsages { get; set; } = [];

        /// <summary>
        /// 配置分类树外键和排序索引。
        /// </summary>
        public void Configure(EntityTypeBuilder<FileCategory> builder)
        {
            builder
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasIndex(x => new
            {
                x.OwnerUserId,
                x.ParentId,
                x.Sort
            });
        }
    }
}
