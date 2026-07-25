using log4net;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Files;

namespace UzonMail.CorePlugin.Services.HostedServices;

/// <summary>
/// 在服务启动后恢复中断的文件删除，并清理无数据库记录的暂存与内容文件。
/// </summary>
public sealed class FileStorageRecoveryService(SqlContext db, FileStoreService fileStoreService)
    : IScopedServiceAfterStarting
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(FileStorageRecoveryService));

    public int Order => 1_100;

    /// <summary>
    /// 数据库迁移完成后，对每个文件桶执行一次确定性恢复。
    /// </summary>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buckets = await db.FileBuckets.AsNoTracking().ToListAsync(stoppingToken);
        var fileObjects = await db
            .FileObjects.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.FileBucket)
            .ToListAsync(stoppingToken);

        foreach (var bucket in buckets)
        {
            stoppingToken.ThrowIfCancellationRequested();
            RecoverTrashFiles(bucket, fileObjects);
            DeleteDirectoryFiles(Path.Combine(bucket.RootDir, ".staging"));
            DeleteOrphanObjectFiles(bucket, fileObjects);
        }
    }

    private void RecoverTrashFiles(FileBucket bucket, IReadOnlyCollection<FileObject> fileObjects)
    {
        var trashDirectory = Path.Combine(bucket.RootDir, ".trash");
        if (!Directory.Exists(trashDirectory))
            return;

        foreach (var trashPath in Directory.EnumerateFiles(trashDirectory))
        {
            try
            {
                var objectId = Path.GetFileName(trashPath).Split('_', 2)[0];
                var fileObject = fileObjects.FirstOrDefault(x =>
                    x.FileBucketId == bucket.Id && x.ObjectId == objectId
                );
                if (fileObject is null || fileObject.IsDeleted)
                {
                    File.Delete(trashPath);
                    continue;
                }

                var originalPath = fileStoreService.GetFileFullPath(fileObject);
                if (File.Exists(originalPath))
                {
                    File.Delete(trashPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
                File.Move(trashPath, originalPath);
            }
            catch (IOException exception)
            {
                Logger.Warn($"恢复文件回收暂存项失败：{trashPath}", exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                Logger.Warn($"无权限恢复文件回收暂存项：{trashPath}", exception);
            }
        }
    }

    private static void DeleteOrphanObjectFiles(
        FileBucket bucket,
        IReadOnlyCollection<FileObject> fileObjects
    )
    {
        var objectsDirectory = Path.Combine(bucket.RootDir, "objects");
        if (!Directory.Exists(objectsDirectory))
            return;

        var activePaths = fileObjects
            .Where(x =>
                x.FileBucketId == bucket.Id
                && !x.IsDeleted
                && x.StorageState == FileObjectStorageState.Ready
            )
            .Select(x => Path.GetFullPath(Path.Combine(bucket.RootDir, x.Path)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (
            var objectPath in Directory.EnumerateFiles(
                objectsDirectory,
                "*",
                SearchOption.AllDirectories
            )
        )
        {
            if (!activePaths.Contains(Path.GetFullPath(objectPath)))
                TryDeleteFile(objectPath);
        }
    }

    private static void DeleteDirectoryFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return;
        foreach (
            var filePath in Directory.EnumerateFiles(
                directoryPath,
                "*",
                SearchOption.AllDirectories
            )
        )
            TryDeleteFile(filePath);
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (IOException exception)
        {
            Logger.Warn($"清理文件失败：{filePath}", exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            Logger.Warn($"无权限清理文件：{filePath}", exception);
        }
    }
}
