using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 管理用户逻辑文件的查询、分类、命名和删除生命周期。
    /// </summary>
    public sealed class FileUsageService(
        SqlContext db,
        FileStoreService fileStoreService,
        FileCategoryService categoryService,
        FileOperationLockService operationLockService
    ) : IScopedService
    {
        /// <summary>
        /// 获取符合分类和搜索条件的用户文件数量。
        /// </summary>
        public async Task<int> GetCountAsync(
            long userId,
            long? categoryId,
            string? filter,
            CancellationToken cancellationToken = default
        ) => await ApplyFilter(userId, categoryId, filter).CountAsync(cancellationToken);

        /// <summary>
        /// 分页获取用户文件列表。
        /// </summary>
        public async Task<List<FileUsageListItem>> GetDataAsync(
            long userId,
            long? categoryId,
            string? filter,
            Pagination pagination,
            CancellationToken cancellationToken = default
        )
        {
            return await ApplyFilter(userId, categoryId, filter)
                .Include(x => x.FileObject)
                .Page(pagination)
                .Select(x => new FileUsageListItem(
                    x.Id,
                    x.CategoryId,
                    x.FileName,
                    x.DisplayName,
                    x.CreateDate,
                    x.FileObject.Sha256,
                    x.FileObject.Size,
                    x.ReferenceCount
                ))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 修改逻辑文件显示名称，并维持 Excel 名称唯一性。
        /// </summary>
        public async Task RenameAsync(
            long userId,
            long fileUsageId,
            string displayName,
            CancellationToken cancellationToken = default
        )
        {
            var usage = await GetOwnedUsageAsync(userId, fileUsageId, cancellationToken);
            var normalizedDisplayName = displayName.Trim();
            if (string.IsNullOrEmpty(normalizedDisplayName))
                normalizedDisplayName = usage.FileName;
            if (normalizedDisplayName.Length > 255)
                throw new KnownException("文件显示名称不能超过 255 个字符");

            var displayNameKey = FileStoreService.NormalizeDisplayName(normalizedDisplayName);
            if (
                await db.FileUsages.AnyAsync(
                    x =>
                        x.OwnerUserId == userId
                        && x.Id != fileUsageId
                        && x.DisplayNameKey == displayNameKey,
                    cancellationToken
                )
            )
                throw new KnownException("文件显示名称重复");

            usage.DisplayName = normalizedDisplayName;
            usage.DisplayNameKey = displayNameKey;
            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量移动用户文件到指定分类。
        /// </summary>
        public async Task MoveToCategoryAsync(
            long userId,
            IReadOnlyCollection<long> fileUsageIds,
            long categoryId,
            CancellationToken cancellationToken = default
        )
        {
            var ids = NormalizeIds(fileUsageIds);
            await categoryService.GetOwnedCategoryAsync(userId, categoryId, cancellationToken);
            var usages = await db
                .FileUsages.Where(x => x.OwnerUserId == userId && ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
            if (usages.Count != ids.Count)
                throw new KnownException("部分文件不存在或无权操作");
            usages.ForEach(x => x.CategoryId = categoryId);
            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 原子软删除一批未被发送任务引用的逻辑文件。
        /// </summary>
        public async Task<FileUsageDeletionResult> DeleteAsync(
            long userId,
            IReadOnlyCollection<long> fileUsageIds,
            CancellationToken cancellationToken = default
        )
        {
            var ids = NormalizeIds(fileUsageIds);
            var usages = await db
                .FileUsages.Where(x => x.OwnerUserId == userId && ids.Contains(x.Id))
                .Include(x => x.FileObject)
                .ThenInclude(x => x.FileBucket)
                .ToListAsync(cancellationToken);
            if (usages.Count != ids.Count)
                throw new KnownException("部分文件不存在或无权操作");

            var referencedUsageNames = usages
                .Where(x => x.ReferenceCount > 0)
                .Select(x => x.DisplayName)
                .ToList();
            if (referencedUsageNames.Count > 0)
                throw new KnownException(
                    $"以下文件正在被发送任务引用，不能删除：{string.Join("、", referencedUsageNames.Distinct())}"
                );

            var fileObjectIds = usages.Select(x => x.FileObjectId).Distinct().ToList();
            var objectIdsStillUsed = await db
                .FileUsages.Where(x =>
                    fileObjectIds.Contains(x.FileObjectId) && !ids.Contains(x.Id)
                )
                .Select(x => x.FileObjectId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var objectsToDelete = usages
                .Select(x => x.FileObject)
                .DistinctBy(x => x.Id)
                .Where(x => !objectIdsStillUsed.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToList();

            var locks = new List<IDisposable>();
            var stagedDeletions = new List<StagedFileDeletion>();
            try
            {
                foreach (var fileObject in objectsToDelete)
                    locks.Add(
                        await operationLockService.AcquireAsync(
                            $"file-object:{fileObject.Sha256}",
                            cancellationToken
                        )
                    );
                var objectIdsReusedWhileWaiting = await db
                    .FileUsages.AsNoTracking()
                    .Where(x => fileObjectIds.Contains(x.FileObjectId) && !ids.Contains(x.Id))
                    .Select(x => x.FileObjectId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                objectsToDelete = objectsToDelete
                    .Where(x => !objectIdsReusedWhileWaiting.Contains(x.Id))
                    .ToList();
                foreach (var fileObject in objectsToDelete)
                {
                    var stagedDeletion = fileStoreService.StageFileDeletion(fileObject);
                    if (stagedDeletion is not null)
                        stagedDeletions.Add(stagedDeletion);
                }

                await using var transaction = await db.Database.BeginTransactionAsync(
                    cancellationToken
                );
                try
                {
                    foreach (var usage in usages)
                    {
                        usage.IsDeleted = true;
                        usage.DisplayNameKey = null;
                    }
                    objectsToDelete.ForEach(x => x.IsDeleted = true);
                    await db
                        .FileReaders.Where(x =>
                            x.UserId == userId && fileObjectIds.Contains(x.FileObjectId)
                        )
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(x => x.IsDeleted, true),
                            cancellationToken
                        );
                    await db.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch
            {
                foreach (var stagedDeletion in stagedDeletions)
                    FileStoreService.RestoreStagedDeletion(stagedDeletion);
                throw;
            }
            finally
            {
                foreach (var operationLock in locks)
                    operationLock.Dispose();
            }

            var pendingCleanupCount = stagedDeletions.Count(x =>
                !FileStoreService.DeleteStagedFile(x)
            );
            return new FileUsageDeletionResult(ids, pendingCleanupCount);
        }

        private IQueryable<FileUsage> ApplyFilter(long userId, long? categoryId, string? filter)
        {
            var query = db.FileUsages.AsNoTracking().Where(x => x.OwnerUserId == userId);
            if (categoryId is > 0)
                query = query.Where(x => x.CategoryId == categoryId);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var normalizedFilter = filter.Trim();
                query = query.Where(x =>
                    x.DisplayName.Contains(normalizedFilter)
                    || x.FileName.Contains(normalizedFilter)
                );
            }
            return query;
        }

        private async Task<FileUsage> GetOwnedUsageAsync(
            long userId,
            long fileUsageId,
            CancellationToken cancellationToken
        ) =>
            await db.FileUsages.FirstOrDefaultAsync(
                x => x.Id == fileUsageId && x.OwnerUserId == userId,
                cancellationToken
            ) ?? throw new KnownException("文件不存在或无权操作");

        private static List<long> NormalizeIds(IReadOnlyCollection<long> fileUsageIds)
        {
            var ids = fileUsageIds.Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
                throw new KnownException("请选择需要操作的文件");
            return ids;
        }
    }

    /// <summary>
    /// 文件管理列表返回项。
    /// </summary>
    public sealed record FileUsageListItem(
        long Id,
        long CategoryId,
        string FileName,
        string DisplayName,
        DateTime CreateDate,
        string Sha256,
        long Size,
        long ReferenceCount
    );

    /// <summary>
    /// 批量删除结果。
    /// </summary>
    public sealed record FileUsageDeletionResult(
        IReadOnlyList<long> DeletedIds,
        int PendingPhysicalCleanupCount
    );
}
