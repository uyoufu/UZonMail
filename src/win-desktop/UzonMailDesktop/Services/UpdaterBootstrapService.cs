using System.IO;
using System.Linq;
using UzonMailDesktop.Localization;

namespace UzonMailDesktop.Services;

/// <summary>
/// 在桌面端启动前安装随应用包交付的新更新器
/// </summary>
internal static class UpdaterBootstrapService
{
    private const string TemporaryUpdaterDirectoryName = "UpdaterTmp";
    private const string UpdaterDirectoryName = "Updater";
    private const int MaximumFileReplacementRetryCount = 8;
    private static readonly TimeSpan InitialFileReplacementRetryDelay = TimeSpan.FromMilliseconds(
        100
    );
    private static readonly TimeSpan MaximumFileReplacementRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// 将临时更新器复制到正式目录，成功后移除临时文件。
    /// 更新器启动桌面端后会短暂持有自身的可执行文件，因此此处等待其退出后再替换。
    /// </summary>
    public static async Task InstallPendingUpdaterAsync(
        string applicationDirectory,
        IDesktopLocalizationService localization
    )
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
                throw new InvalidOperationException(
                    localization.GetText(DesktopTextKey.UpdaterInvalidPath, relativePath)
                );

            var targetFile = Path.Combine(targetDirectory, relativePath);
            await ReplaceFileAsync(sourceFile, targetFile);
        }

        Directory.Delete(sourceDirectory, recursive: true);
    }

    private static async Task ReplaceFileAsync(string sourceFile, string targetFile)
    {
        for (var retryCount = 0; ; retryCount++)
        {
            var temporaryFile = $"{targetFile}.{Guid.NewGuid():N}.tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                File.Copy(sourceFile, temporaryFile, overwrite: true);
                File.Move(temporaryFile, targetFile, overwrite: true);
                return;
            }
            catch (IOException) when (retryCount < MaximumFileReplacementRetryCount) { }
            catch (UnauthorizedAccessException) when (retryCount < MaximumFileReplacementRetryCount)
            { }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }

            await Task.Delay(GetRetryDelay(retryCount));
        }
    }

    private static TimeSpan GetRetryDelay(int retryCount)
    {
        var delayMilliseconds =
            InitialFileReplacementRetryDelay.TotalMilliseconds * Math.Pow(2, retryCount);
        return TimeSpan.FromMilliseconds(
            Math.Min(delayMilliseconds, MaximumFileReplacementRetryDelay.TotalMilliseconds)
        );
    }
}
