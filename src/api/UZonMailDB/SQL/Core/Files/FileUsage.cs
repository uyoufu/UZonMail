using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Organization;

namespace UzonMail.DB.SQL.Core.Files
{
    /// <summary>
    /// 逻辑文件的所有权范围。
    /// 收件缓存不应出现在用户可管理的文件列表中，也不能被用户主动删除。
    /// </summary>
    public enum FileUsageScope
    {
        /// <summary>
        /// 用户主动上传和管理的文件。
        /// </summary>
        UserManaged = 0,

        /// <summary>
        /// 后台为收件正文或附件创建的缓存文件。
        /// </summary>
        IncomingMailCache = 1,
    }

    /// <summary>
    /// 用户可管理和引用的逻辑文件。
    /// </summary>
    public class FileUsage : SqlId, IEntityTypeConfiguration<FileUsage>
    {
        [NotMapped]
        public long __fileUsageId { get; set; }

        public long OwnerUserId { get; set; }
        public User OwnerUser { get; set; } = null!;

        public long CategoryId { get; set; }
        public FileCategory Category { get; set; } = null!;

        private string _fileName = string.Empty;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                if (string.IsNullOrEmpty(DisplayName))
                    DisplayName = value;
            }
        }

        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 用于 Excel 附件名称匹配的标准化名称，软删除后置空。
        /// </summary>
        public string? DisplayNameKey { get; set; }

        public long FileObjectId { get; set; }
        public FileObject FileObject { get; set; } = null!;

        public bool IsPublic { get; set; }

        /// <summary>
        /// 文件的所有权范围。
        /// </summary>
        public FileUsageScope Scope { get; set; } = FileUsageScope.UserManaged;

        /// <summary>
        /// 发件项、收件邮件等业务实体对该逻辑文件的引用数量。
        /// </summary>
        public long ReferenceCount { get; set; }

        /// <summary>
        /// 配置逻辑文件的唯一性与列表查询索引。
        /// </summary>
        public void Configure(EntityTypeBuilder<FileUsage> builder)
        {
            builder.HasIndex(x => new { x.OwnerUserId, x.FileObjectId }).IsUnique();
            builder.HasIndex(x => new { x.OwnerUserId, x.DisplayNameKey }).IsUnique();
            builder.HasIndex(x => new
            {
                x.OwnerUserId,
                x.CategoryId,
                x.CreateDate
            });
            builder.HasIndex(x => new
            {
                x.OwnerUserId,
                x.Scope,
                x.CreateDate
            });
        }
    }
}
