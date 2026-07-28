using System.IO;
using System.Linq;

namespace UzonMailDesktop.Services;

/// <summary>
/// 在桌面端启动前安装随应用包交付的新更新器
/// </summary>
internal static class UpdaterBootstrapService
{
    private const string TemporaryUpdaterDirectoryName = "UpdaterTmp";
    private const string UpdaterDirectoryName = "Updater";

    /// <summary>
    /// 将临时更新器复制到正式目录，成功后移除临时文件
    /// </summary>
    public static void InstallPendingUpdater(string applicationDirectory)
    {
        var sourceDirectory = Path.Combine(applicationDirectory, TemporaryUpdaterDirectoryName);
        if (
            !Directory.Exists(sourceDirectory)
            || !Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).Any()
        )
            return;

        var targetDirectory = Path.Combine(applicationDirectory, UpdaterDirectoryName);
        foreach (
            var sourceFile in Directory.EnumerateFiles(
                sourceDirectory,
                "*",
                SearchOption.AllDirectories
            )
        )
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            if (
                Path.IsPathRooted(relativePath)
                || relativePath.StartsWith("..", StringComparison.Ordinal)
            )
                throw new InvalidOperationException($"更新器包含非法文件路径：{relativePath}");

            var targetFile = Path.Combine(targetDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            var temporaryFile = $"{targetFile}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.Copy(sourceFile, temporaryFile, overwrite: true);
                File.Move(temporaryFile, targetFile, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }

        Directory.Delete(sourceDirectory, recursive: true);
    }
}
