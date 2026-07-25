using System.ComponentModel.DataAnnotations;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.DB.SQL.Core.Files;

namespace UzonMail.CorePlugin.Controllers.Files.DTOs
{
    /// <summary>
    /// 批量删除用户文件请求。
    /// </summary>
    public sealed class DeleteFileUsagesDto
    {
        [MinLength(1)]
        public List<long> FileUsageIds { get; set; } = [];
    }

    /// <summary>
    /// 批量移动用户文件请求。
    /// </summary>
    public sealed class MoveFileUsagesDto
    {
        [MinLength(1)]
        public List<long> FileUsageIds { get; set; } = [];

        [Range(1, long.MaxValue)]
        public long CategoryId { get; set; }
    }

    /// <summary>
    /// 创建文件分类请求。
    /// </summary>
    public sealed class CreateFileCategoryDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public long? ParentId { get; set; }
    }

    /// <summary>
    /// 重命名文件分类请求。
    /// </summary>
    public sealed class RenameFileCategoryDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 移动文件分类请求。
    /// </summary>
    public sealed class MoveFileCategoryDto
    {
        [Range(1, long.MaxValue)]
        public long TargetCategoryId { get; set; }

        public FileCategoryPlacement Placement { get; set; }
    }

    /// <summary>
    /// 文件分类树节点响应。
    /// </summary>
    public sealed record FileCategoryDto(
        long Id,
        long? ParentId,
        string Name,
        long Sort,
        bool IsDefault
    )
    {
        public static FileCategoryDto FromEntity(FileCategory category) =>
            new(category.Id, category.ParentId, category.Name, category.Sort, category.IsDefault);
    }
}
