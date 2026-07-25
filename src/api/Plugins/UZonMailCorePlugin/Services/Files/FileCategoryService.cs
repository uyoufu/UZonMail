using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 管理用户文件分类树及默认分类约束。
    /// </summary>
    public sealed class FileCategoryService(
        SqlContext db,
        FileOperationLockService operationLockService
    ) : IScopedService
    {
        /// <summary>
        /// 获取用户分类，并确保 Default 分类存在。
        /// </summary>
        public async Task<List<FileCategory>> GetAllAsync(
            long userId,
            CancellationToken cancellationToken = default
        )
        {
            await GetOrCreateDefaultAsync(userId, cancellationToken);
            return await db
                .FileCategories.AsNoTracking()
                .Where(x => x.OwnerUserId == userId)
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.Sort)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取或创建用户唯一的 Default 分类。
        /// </summary>
        public async Task<FileCategory> GetOrCreateDefaultAsync(
            long userId,
            CancellationToken cancellationToken = default
        )
        {
            var existing = await db.FileCategories.FirstOrDefaultAsync(
                x => x.OwnerUserId == userId && x.IsDefault,
                cancellationToken
            );
            if (existing is not null)
                return existing;

            using var operationLock = await operationLockService.AcquireAsync(
                $"file-category-default:{userId}",
                cancellationToken
            );
            existing = await db.FileCategories.FirstOrDefaultAsync(
                x => x.OwnerUserId == userId && x.IsDefault,
                cancellationToken
            );
            if (existing is not null)
                return existing;

            var defaultCategory = new FileCategory
            {
                OwnerUserId = userId,
                Name = FileCategory.DefaultName,
                IsDefault = true,
                Sort = 0,
            };
            db.FileCategories.Add(defaultCategory);
            await db.SaveChangesAsync(cancellationToken);
            return defaultCategory;
        }

        /// <summary>
        /// 验证并返回当前用户拥有的分类；未指定分类时返回 Default。
        /// </summary>
        public async Task<FileCategory> GetOwnedCategoryAsync(
            long userId,
            long? categoryId,
            CancellationToken cancellationToken = default
        )
        {
            if (categoryId is null or <= 0)
                return await GetOrCreateDefaultAsync(userId, cancellationToken);

            return await db.FileCategories.FirstOrDefaultAsync(
                    x => x.Id == categoryId && x.OwnerUserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("文件分类不存在");
        }

        /// <summary>
        /// 新建根分类或子分类。
        /// </summary>
        public async Task<FileCategory> CreateAsync(
            long userId,
            string name,
            long? parentId,
            CancellationToken cancellationToken = default
        )
        {
            var normalizedName = NormalizeName(name);
            parentId = parentId is > 0 ? parentId : null;
            if (parentId is > 0)
            {
                var parent = await GetOwnedCategoryAsync(userId, parentId, cancellationToken);
                if (parent.IsDefault)
                    throw new KnownException("Default 分类下不允许创建子分类");
            }

            await EnsureSiblingNameAvailableAsync(
                userId,
                parentId,
                normalizedName,
                null,
                cancellationToken
            );
            var nextSort =
                await db
                    .FileCategories.Where(x => x.OwnerUserId == userId && x.ParentId == parentId)
                    .Select(x => (long?)x.Sort)
                    .MaxAsync(cancellationToken) ?? 0;
            var category = new FileCategory
            {
                OwnerUserId = userId,
                ParentId = parentId,
                Name = normalizedName,
                Sort = nextSort + 1,
            };
            db.FileCategories.Add(category);
            await db.SaveChangesAsync(cancellationToken);
            return category;
        }

        /// <summary>
        /// 重命名用户分类。
        /// </summary>
        public async Task<FileCategory> RenameAsync(
            long userId,
            long categoryId,
            string name,
            CancellationToken cancellationToken = default
        )
        {
            var category = await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
            if (category.IsDefault)
                throw new KnownException("Default 分类不允许重命名");

            var normalizedName = NormalizeName(name);
            await EnsureSiblingNameAvailableAsync(
                userId,
                category.ParentId,
                normalizedName,
                category.Id,
                cancellationToken
            );
            category.Name = normalizedName;
            await db.SaveChangesAsync(cancellationToken);
            return category;
        }

        /// <summary>
        /// 根据树节点拖拽语义移动分类并重新生成同级排序号。
        /// </summary>
        public async Task MoveAsync(
            long userId,
            long categoryId,
            long targetCategoryId,
            FileCategoryPlacement placement,
            CancellationToken cancellationToken = default
        )
        {
            if (categoryId == targetCategoryId)
                throw new KnownException("分类不能移动到自身");

            var categories = await db
                .FileCategories.Where(x => x.OwnerUserId == userId)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);
            var category =
                categories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KnownException("文件分类不存在");
            var target =
                categories.FirstOrDefault(x => x.Id == targetCategoryId)
                ?? throw new KnownException("目标文件分类不存在");
            if (category.IsDefault || target.IsDefault)
                throw new KnownException("Default 分类不允许参与拖拽");

            var newParentId =
                placement == FileCategoryPlacement.Inside ? target.Id : target.ParentId;
            if (newParentId == category.Id || IsDescendant(categories, category.Id, newParentId))
                throw new KnownException("分类不能移动到自身的子分类中");

            var oldParentId = category.ParentId;
            category.ParentId = newParentId;
            ReorderSiblings(categories, oldParentId, null, null, FileCategoryPlacement.Inside);
            ReorderSiblings(categories, newParentId, category, target, placement);
            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 软删除空分类。
        /// </summary>
        public async Task DeleteAsync(
            long userId,
            long categoryId,
            CancellationToken cancellationToken = default
        )
        {
            var category = await GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
            if (category.IsDefault)
                throw new KnownException("Default 分类不允许删除");
            var hasChildren = await db.FileCategories.AnyAsync(
                x => x.OwnerUserId == userId && x.ParentId == categoryId,
                cancellationToken
            );
            var hasFiles = await db.FileUsages.AnyAsync(
                x => x.OwnerUserId == userId && x.CategoryId == categoryId,
                cancellationToken
            );
            if (hasChildren || hasFiles)
                throw new KnownException("非空分类不允许删除");

            category.IsDeleted = true;
            await db.SaveChangesAsync(cancellationToken);
        }

        private static string NormalizeName(string name)
        {
            var result = name.Trim();
            if (string.IsNullOrEmpty(result))
                throw new KnownException("分类名称不能为空");
            if (result.Length > 100)
                throw new KnownException("分类名称不能超过 100 个字符");
            if (string.Equals(result, FileCategory.DefaultName, StringComparison.OrdinalIgnoreCase))
                throw new KnownException("Default 是系统保留分类名称");
            return result;
        }

        private async Task EnsureSiblingNameAvailableAsync(
            long userId,
            long? parentId,
            string name,
            long? excludedCategoryId,
            CancellationToken cancellationToken
        )
        {
            var normalizedName = name.ToUpperInvariant();
            var names = await db
                .FileCategories.Where(x =>
                    x.OwnerUserId == userId && x.ParentId == parentId && x.Id != excludedCategoryId
                )
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);
            if (names.Any(x => x.ToUpperInvariant() == normalizedName))
                throw new KnownException("同级分类名称重复");
        }

        private static bool IsDescendant(
            IReadOnlyCollection<FileCategory> categories,
            long categoryId,
            long? possibleDescendantId
        )
        {
            var currentId = possibleDescendantId;
            while (currentId is > 0)
            {
                if (currentId == categoryId)
                    return true;
                currentId = categories.FirstOrDefault(x => x.Id == currentId)?.ParentId;
            }
            return false;
        }

        private static void ReorderSiblings(
            IReadOnlyCollection<FileCategory> categories,
            long? parentId,
            FileCategory? movedCategory,
            FileCategory? targetCategory,
            FileCategoryPlacement placement
        )
        {
            var siblings = categories
                .Where(x => x.ParentId == parentId && x != movedCategory && !x.IsDefault)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Id)
                .ToList();
            if (movedCategory is not null)
            {
                var targetIndex = targetCategory is null
                    ? siblings.Count
                    : siblings.IndexOf(targetCategory);
                if (targetIndex < 0)
                    targetIndex = siblings.Count;
                else if (placement == FileCategoryPlacement.After)
                    targetIndex++;
                siblings.Insert(targetIndex, movedCategory);
            }
            for (var index = 0; index < siblings.Count; index++)
                siblings[index].Sort = index + 1;
        }
    }
}
