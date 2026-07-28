using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UzonMailUpdater.Models;
using UzonMailUpdater.Services;

namespace UzonMailDotNET.Test.UzonMail.Desktop;

/// <summary>
/// 验证桌面端更新清单的生成和多位置写入
/// </summary>
[TestClass]
public sealed class AppPackageServiceTests
{
    [TestMethod]
    public async Task WriteAsync_WritesIdenticalManifestToEveryOutput()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var executablePath = Path.Combine(temporaryDirectory, "UzonMailDesktop.exe");
            File.Copy(typeof(AppPackageService).Assembly.Location, executablePath);
            await File.WriteAllTextAsync(
                Path.Combine(temporaryDirectory, "content.txt"),
                "updated-content"
            );

            var packageService = new AppPackageService();
            var manifest = await packageService.CreateAsync(temporaryDirectory);
            var latestManifest = Path.Combine(temporaryDirectory, "updates", "latest.json");
            var versionManifest = Path.Combine(
                temporaryDirectory,
                "updates",
                $"{manifest.Version}.json"
            );
            await packageService.WriteAsync(
                manifest,
                temporaryDirectory,
                [latestManifest, versionManifest]
            );

            var latestContent = await File.ReadAllTextAsync(latestManifest);
            Assert.AreEqual(latestContent, await File.ReadAllTextAsync(versionManifest));
            var writtenManifest = JsonSerializer.Deserialize<AppPackage>(
                latestContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            Assert.IsNotNull(writtenManifest);
            Assert.IsTrue(writtenManifest.Dependencies.ContainsKey("content.txt"));
            Assert.IsFalse(writtenManifest.Dependencies.ContainsKey("appPackage.json"));
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}
