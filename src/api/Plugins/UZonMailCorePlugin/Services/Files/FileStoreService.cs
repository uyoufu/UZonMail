using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 负责文件内容的校验、内容寻址持久化和可恢复磁盘操作。
    /// </summary>
    public sealed class FileStoreService(
        SqlContext db,
        IWebHostEnvironment env,
        FileCategoryService categoryService,
        FileOperationLockService operationLockService
    ) : IScopedService
    {
        private const string StagingDirectoryName = ".staging";
        private const string TrashDirectoryName = ".trash";

        /// <summary>
        /// 保存上传内容并返回当前用户的逻辑文件。
        /// </summary>
        public async Task<FileUploadResult> UploadFileObjectAsync(
            long userId,
            ObjectFileUploaderBody fileParams,
            CancellationToken cancellationToken = default
        )
        {
            var uploadedFile = fileParams.File ?? throw new KnownException("未找到上传文件");
            var expectedSha256 = NormalizeSha256(fileParams.Sha256);
            var bucket = await GetDefaultBucketAsync(cancellationToken);
            var stagingPath = GetStagingPath(bucket, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.GetDirectoryName(stagingPath)!);

            try
            {
                await using (
                    var output = new FileStream(
                        stagingPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        FileOptions.Asynchronous
                    )
                )
                {
                    await uploadedFile.CopyToAsync(output, cancellationToken);
                }

                var actualSha256 = await ComputeSha256Async(stagingPath, cancellationToken);
                if (!string.Equals(expectedSha256, actualSha256, StringComparison.Ordinal))
                    throw new KnownException("上传文件的 SHA-256 校验失败");

                using var operationLock = await operationLockService.AcquireAsync(
                    $"file-object:{actualSha256}",
                    cancellationToken
                );
                await using var transaction = await db.Database.BeginTransactionAsync(
                    cancellationToken
                );
                string? newlyStoredPath = null;
                try
                {
                    var fileObject = await db
                        .FileObjects.IgnoreQueryFilters()
                        .Include(x => x.FileBucket)
                        .FirstOrDefaultAsync(x => x.Sha256 == actualSha256, cancellationToken);
                    var isExistingObject =
                        fileObject is not null
                        && !fileObject.IsDeleted
                        && fileObject.StorageState == FileObjectStorageState.Ready
                        && File.Exists(GetFileFullPath(fileObject));

                    if (fileObject is null)
                    {
                        fileObject = new FileObject
                        {
                            FileBucketId = bucket.Id,
                            FileBucket = bucket,
                            Sha256 = actualSha256,
                            Path = GetObjectRelativePath(actualSha256),
                            Size = uploadedFile.Length,
                            LastModifyDate = fileParams.LastModifyDate,
                            StorageState = FileObjectStorageState.Pending,
                        };
                        db.FileObjects.Add(fileObject);
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    if (!isExistingObject)
                    {
                        newlyStoredPath = GetFullPath(bucket, GetObjectRelativePath(actualSha256));
                        Directory.CreateDirectory(Path.GetDirectoryName(newlyStoredPath)!);
                        if (File.Exists(newlyStoredPath))
                            File.Delete(newlyStoredPath);
                        File.Move(stagingPath, newlyStoredPath);
                        fileObject.FileBucketId = bucket.Id;
                        fileObject.FileBucket = bucket;
                        fileObject.Path = GetObjectRelativePath(actualSha256);
                        fileObject.Size = uploadedFile.Length;
                        fileObject.LastModifyDate = fileParams.LastModifyDate;
                        fileObject.StorageState = FileObjectStorageState.Ready;
                        fileObject.SetStatusNormal();
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        File.Delete(stagingPath);
                    }

                    var (usage, isExistingUsage) = await GetOrCreateFileUsageAsync(
                        userId,
                        uploadedFile.FileName,
                        fileObject,
                        fileParams.CategoryId,
                        fileParams.IsPublic,
                        cancellationToken
                    );
                    await transaction.CommitAsync(cancellationToken);
                    return new FileUploadResult(usage.Id, usage.CategoryId, isExistingUsage);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    db.ChangeTracker.Clear();
                    if (newlyStoredPath is not null && File.Exists(newlyStoredPath))
                        File.Delete(newlyStoredPath);
                    throw;
                }
            }
            finally
            {
                if (File.Exists(stagingPath))
                    File.Delete(stagingPath);
            }
        }

        /// <summary>
        /// 按内容哈希获取已有对象。
        /// </summary>
        public async Task<FileObject?> GetExistFileObjectAsync(
            string sha256,
            CancellationToken cancellationToken = default
        )
        {
            var normalizedSha256 = NormalizeSha256(sha256);
            return await db.FileObjects.FirstOrDefaultAsync(
                x => x.Sha256 == normalizedSha256 && x.StorageState == FileObjectStorageState.Ready,
                cancellationToken
            );
        }

        /// <summary>
        /// 为已存在的物理对象获取或创建当前用户的逻辑文件。
        /// </summary>
        public async Task<FileUsage> GetOrCreateFileUsageAsync(
            long userId,
            string fileName,
            string sha256,
            CancellationToken cancellationToken = default
        )
        {
            var fileObject =
                await GetExistFileObjectAsync(sha256, cancellationToken)
                ?? throw new FileNotFoundException("文件对象不存在");
            var (usage, _) = await GetOrCreateFileUsageAsync(
                userId,
                fileName,
                fileObject,
                null,
                false,
                cancellationToken
            );
            return usage;
        }

        /// <summary>
        /// 获取当前用户可读取的私有文件路径。
        /// </summary>
        public async Task<string> GetOwnedFileFullPathAsync(
            long fileUsageId,
            long userId,
            CancellationToken cancellationToken = default
        )
        {
            var usage = await GetReadableUsageAsync(
                fileUsageId,
                x => x.OwnerUserId == userId,
                cancellationToken
            );
            return GetExistingFileFullPath(usage.FileObject);
        }

        /// <summary>
        /// 获取可匿名读取的公共文件路径。
        /// </summary>
        public async Task<string> GetPublicFileFullPathAsync(
            long fileUsageId,
            CancellationToken cancellationToken = default
        )
        {
            var usage = await GetReadableUsageAsync(
                fileUsageId,
                x => x.IsPublic,
                cancellationToken
            );
            return GetExistingFileFullPath(usage.FileObject);
        }

        /// <summary>
        /// 获取已加载存储桶的文件对象绝对路径。
        /// </summary>
        public string GetFileFullPath(FileObject fileObject)
        {
            ArgumentNullException.ThrowIfNull(fileObject);
            if (fileObject.FileBucket is null)
                throw new KnownException("文件对象缺少存储桶信息");
            return GetFullPath(fileObject.FileBucket, fileObject.Path);
        }

        /// <summary>
        /// 将待删除文件原子移动到回收暂存区。
        /// </summary>
        public StagedFileDeletion? StageFileDeletion(FileObject fileObject)
        {
            var originalPath = GetFileFullPath(fileObject);
            if (!File.Exists(originalPath))
                return null;
            var trashPath = GetFullPath(
                fileObject.FileBucket,
                Path.Combine(TrashDirectoryName, $"{fileObject.ObjectId}_{fileObject.Sha256}")
            );
            Directory.CreateDirectory(Path.GetDirectoryName(trashPath)!);
            if (File.Exists(trashPath))
                File.Delete(trashPath);
            File.Move(originalPath, trashPath);
            return new StagedFileDeletion(originalPath, trashPath);
        }

        /// <summary>
        /// 恢复数据库回滚所对应的磁盘文件。
        /// </summary>
        public static void RestoreStagedDeletion(StagedFileDeletion stagedDeletion)
        {
            if (!File.Exists(stagedDeletion.TrashPath))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(stagedDeletion.OriginalPath)!);
            if (File.Exists(stagedDeletion.OriginalPath))
                File.Delete(stagedDeletion.OriginalPath);
            File.Move(stagedDeletion.TrashPath, stagedDeletion.OriginalPath);
        }

        /// <summary>
        /// 永久删除已经提交的回收暂存文件。
        /// </summary>
        public static bool DeleteStagedFile(StagedFileDeletion stagedDeletion)
        {
            try
            {
                if (File.Exists(stagedDeletion.TrashPath))
                    File.Delete(stagedDeletion.TrashPath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// 生成受根目录约束的静态文件保存路径。
        /// </summary>
        public (string FullPath, string RelativeUrl) GenerateStaticFilePath(params string[] paths)
        {
            var staticRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, "data/public"));
            var relativePath = Path.Combine(paths);
            var fullPath = GetContainedPath(staticRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            return (fullPath, $"data/public/{relativePath}".Replace('\\', '/'));
        }

        private async Task<(FileUsage Usage, bool IsExisting)> GetOrCreateFileUsageAsync(
            long userId,
            string fileName,
            FileObject fileObject,
            long? categoryId,
            bool isPublic,
            CancellationToken cancellationToken
        )
        {
            using var usageLock = await operationLockService.AcquireAsync(
                $"file-usage:{userId}:{fileObject.Id}",
                cancellationToken
            );
            var existing = await db
                .FileUsages.IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.OwnerUserId == userId && x.FileObjectId == fileObject.Id,
                    cancellationToken
                );
            if (existing is not null && !existing.IsDeleted)
            {
                if (categoryId is > 0 && existing.CategoryId != categoryId)
                {
                    var targetCategory = await categoryService.GetOwnedCategoryAsync(
                        userId,
                        categoryId,
                        cancellationToken
                    );
                    existing.CategoryId = targetCategory.Id;
                    await db.SaveChangesAsync(cancellationToken);
                }
                return (existing, true);
            }

            var category = await categoryService.GetOwnedCategoryAsync(
                userId,
                categoryId,
                cancellationToken
            );
            var displayName = await GetAvailableDisplayNameAsync(
                userId,
                Path.GetFileName(fileName),
                cancellationToken
            );
            if (existing is null)
            {
                existing = new FileUsage { OwnerUserId = userId, FileObjectId = fileObject.Id, };
                db.FileUsages.Add(existing);
            }
            existing.CategoryId = category.Id;
            existing.FileName = Path.GetFileName(fileName);
            existing.DisplayName = displayName;
            existing.DisplayNameKey = NormalizeDisplayName(displayName);
            existing.IsPublic = isPublic;
            existing.ReferenceCount = 0;
            existing.SetStatusNormal();
            await db.SaveChangesAsync(cancellationToken);
            return (existing, false);
        }

        private async Task<string> GetAvailableDisplayNameAsync(
            long userId,
            string requestedName,
            CancellationToken cancellationToken
        )
        {
            var safeName = string.IsNullOrWhiteSpace(requestedName) ? "file" : requestedName.Trim();
            var extension = Path.GetExtension(safeName);
            var baseName = Path.GetFileNameWithoutExtension(safeName);
            var candidate = safeName;
            for (var suffix = 2; ; suffix++)
            {
                var key = NormalizeDisplayName(candidate);
                if (
                    !await db.FileUsages.AnyAsync(
                        x => x.OwnerUserId == userId && x.DisplayNameKey == key,
                        cancellationToken
                    )
                )
                    return candidate;
                candidate = $"{baseName} ({suffix}){extension}";
            }
        }

        /// <summary>
        /// 标准化用户可见文件名，供上传、重命名和 Excel 引用复用。
        /// </summary>
        public static string NormalizeDisplayName(string displayName) =>
            displayName.Trim().ToUpperInvariant();

        private async Task<FileUsage> GetReadableUsageAsync(
            long fileUsageId,
            System.Linq.Expressions.Expression<Func<FileUsage, bool>> accessPredicate,
            CancellationToken cancellationToken
        )
        {
            return await db
                    .FileUsages.Where(accessPredicate)
                    .Include(x => x.FileObject)
                    .ThenInclude(x => x.FileBucket)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == fileUsageId
                            && x.FileObject.StorageState == FileObjectStorageState.Ready,
                        cancellationToken
                    ) ?? throw new KnownException("文件不存在或无权访问");
        }

        private string GetExistingFileFullPath(FileObject fileObject)
        {
            var fullPath = GetFileFullPath(fileObject);
            if (!File.Exists(fullPath))
                throw new KnownException("文件内容不存在");
            return fullPath;
        }

        private async Task<FileBucket> GetDefaultBucketAsync(CancellationToken cancellationToken)
        {
            return await db.FileBuckets.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken)
                ?? throw new KnownException("未找到默认存储桶");
        }

        private static async Task<string> ComputeSha256Async(
            string filePath,
            CancellationToken cancellationToken
        )
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan
            );
            var hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return Convert.ToHexStringLower(hash);
        }

        private static string NormalizeSha256(string sha256)
        {
            var result = sha256.Trim().ToLowerInvariant();
            if (result.Length != 64 || result.Any(x => !Uri.IsHexDigit(x)))
                throw new KnownException("SHA-256 格式不正确");
            return result;
        }

        private static string GetObjectRelativePath(string sha256) =>
            Path.Combine("objects", sha256[..2], sha256[2..4], sha256);

        private static string GetStagingPath(FileBucket bucket, string operationId) =>
            GetFullPath(bucket, Path.Combine(StagingDirectoryName, operationId));

        private static string GetFullPath(FileBucket bucket, string relativePath) =>
            GetContainedPath(Path.GetFullPath(bucket.RootDir), relativePath);

        private static string GetContainedPath(string rootPath, string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                throw new KnownException("文件相对路径不合法");
            var fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
            var rootWithSeparator =
                rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                throw new KnownException("文件路径超出允许的存储目录");
            return fullPath;
        }
    }

    /// <summary>
    /// 已移动到回收暂存区、等待数据库提交的物理文件。
    /// </summary>
    public sealed record StagedFileDeletion(string OriginalPath, string TrashPath);
}
