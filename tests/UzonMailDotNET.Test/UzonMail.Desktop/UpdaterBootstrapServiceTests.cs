using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UzonMailDesktop.Localization;
using UzonMailDesktop.Services;

namespace UzonMailDotNET.Test.UzonMail.Desktop;

/// <summary>
/// 验证桌面端在更新器退出时安装暂存更新器的行为。
/// </summary>
[TestClass]
public sealed class UpdaterBootstrapServiceTests
{
    private static readonly TimeSpan TemporaryTargetFileLockDuration = TimeSpan.FromMilliseconds(
        250
    );

    [TestMethod]
    public async Task InstallPendingUpdaterAsync_ReplacesFilesAndRemovesStagingDirectory()
    {
        var applicationDirectory = CreateTemporaryDirectory();
        try
        {
            var sourceDirectory = Path.Combine(applicationDirectory, "UpdaterTmp");
            var targetDirectory = Path.Combine(applicationDirectory, "Updater");
            var sourceFile = Path.Combine(sourceDirectory, "nested", "UzonMailUpdater.exe");
            var targetFile = Path.Combine(targetDirectory, "nested", "UzonMailUpdater.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            await File.WriteAllTextAsync(sourceFile, "new updater");
            await File.WriteAllTextAsync(targetFile, "old updater");

            await UpdaterBootstrapService.InstallPendingUpdaterAsync(
                applicationDirectory,
                CreateLocalization(applicationDirectory)
            );

            Assert.AreEqual("new updater", await File.ReadAllTextAsync(targetFile));
            Assert.IsFalse(Directory.Exists(sourceDirectory));
        }
        finally
        {
            DeleteTemporaryDirectory(applicationDirectory);
        }
    }

    [TestMethod]
    public async Task InstallPendingUpdaterAsync_RetriesWhenTargetFileIsTemporarilyLocked()
    {
        var applicationDirectory = CreateTemporaryDirectory();
        try
        {
            var sourceFile = CreatePendingUpdaterFile(applicationDirectory, "new updater");
            var targetFile = CreateInstalledUpdaterFile(applicationDirectory, "old updater");
            using var targetFileLock = new FileStream(
                targetFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            var installation = UpdaterBootstrapService.InstallPendingUpdaterAsync(
                applicationDirectory,
                CreateLocalization(applicationDirectory)
            );
            await Task.Delay(TemporaryTargetFileLockDuration);
            targetFileLock.Dispose();
            await installation;

            Assert.AreEqual("new updater", await File.ReadAllTextAsync(targetFile));
            Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(sourceFile)!));
        }
        finally
        {
            DeleteTemporaryDirectory(applicationDirectory);
        }
    }

    [TestMethod]
    public async Task InstallPendingUpdaterAsync_PreservesStagingDirectoryWhenTargetFileRemainsLocked()
    {
        var applicationDirectory = CreateTemporaryDirectory();
        try
        {
            var sourceFile = CreatePendingUpdaterFile(applicationDirectory, "new updater");
            var targetFile = CreateInstalledUpdaterFile(applicationDirectory, "old updater");
            using var targetFileLock = new FileStream(
                targetFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            Exception? installationException = null;
            try
            {
                await UpdaterBootstrapService.InstallPendingUpdaterAsync(
                    applicationDirectory,
                    CreateLocalization(applicationDirectory)
                );
            }
            catch (IOException exception)
            {
                installationException = exception;
            }
            catch (UnauthorizedAccessException exception)
            {
                installationException = exception;
            }

            Assert.IsNotNull(installationException);
            Assert.IsTrue(File.Exists(sourceFile));
        }
        finally
        {
            DeleteTemporaryDirectory(applicationDirectory);
        }
    }

    private static string CreatePendingUpdaterFile(string applicationDirectory, string content)
    {
        var sourceDirectory = Path.Combine(applicationDirectory, "UpdaterTmp");
        Directory.CreateDirectory(sourceDirectory);
        var sourceFile = Path.Combine(sourceDirectory, "UzonMailUpdater.exe");
        File.WriteAllText(sourceFile, content);
        return sourceFile;
    }

    private static string CreateInstalledUpdaterFile(string applicationDirectory, string content)
    {
        var targetDirectory = Path.Combine(applicationDirectory, "Updater");
        Directory.CreateDirectory(targetDirectory);
        var targetFile = Path.Combine(targetDirectory, "UzonMailUpdater.exe");
        File.WriteAllText(targetFile, content);
        return targetFile;
    }

    private static IDesktopLocalizationService CreateLocalization(string applicationDirectory) =>
        new DesktopLocalizationService(
            new DesktopLocalizationSettingsStore(
                Path.Combine(applicationDirectory, "appsettings.Production.json")
            ),
            CultureInfo.GetCultureInfo("en-US"),
            configuredLocale: null
        );

    private static string CreateTemporaryDirectory()
    {
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "UzonMailDesktopTests",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(temporaryDirectory);
        return temporaryDirectory;
    }

    private static void DeleteTemporaryDirectory(string temporaryDirectory)
    {
        if (Directory.Exists(temporaryDirectory))
            Directory.Delete(temporaryDirectory, recursive: true);
    }
}
