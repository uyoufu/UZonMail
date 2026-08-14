using System.Security.Cryptography;
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
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "UzonMailUpdater.runtimeconfig.json",
                """
                {
                  "runtimeOptions": {
                    "framework": {
                      "name": "Microsoft.NETCore.App",
                      "version": "10.0.0"
                    }
                  }
                }
                """
            );
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "service/UzonMailService.runtimeconfig.json",
                """
                {
                  "runtimeOptions": {
                    "frameworks": [
                      {
                        "name": "Microsoft.NETCore.App",
                        "version": "10.0.0"
                      },
                      {
                        "name": "Microsoft.AspNetCore.App",
                        "version": "10.0.0"
                      }
                    ]
                  }
                }
                """
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
            Assert.HasCount(2, writtenManifest.Env);
            Assert.AreEqual("10.0.0", writtenManifest.Env["Microsoft.NETCore.App"]);
            Assert.AreEqual("10.0.0", writtenManifest.Env["Microsoft.AspNetCore.App"]);
            Assert.IsTrue(writtenManifest.Dependencies.ContainsKey("content.txt"));
            Assert.IsFalse(writtenManifest.Dependencies.ContainsKey("appPackage.json"));
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task CreateAsync_ConflictingFrameworkVersions_ThrowsInvalidDataException()
    {
        var temporaryDirectory = CreateProjectDirectory();
        try
        {
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "first.runtimeconfig.json",
                CreateSingleFrameworkJson("Microsoft.NETCore.App", "10.0.0")
            );
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "nested/second.runtimeconfig.json",
                CreateSingleFrameworkJson("Microsoft.NETCore.App", "10.0.1")
            );

            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => new AppPackageService().CreateAsync(temporaryDirectory)
            );

            StringAssert.Contains(exception.Message, "Microsoft.NETCore.App");
            StringAssert.Contains(exception.Message, "10.0.0");
            StringAssert.Contains(exception.Message, "10.0.1");
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task CreateAsync_MissingRuntimeConfig_ThrowsInvalidDataException()
    {
        var temporaryDirectory = CreateProjectDirectory();
        try
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => new AppPackageService().CreateAsync(temporaryDirectory)
            );

            StringAssert.Contains(exception.Message, "runtimeconfig");
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task CreateAsync_WithLinuxPackage_AddsVerifiedArtifact()
    {
        var temporaryDirectory = CreateProjectDirectory();
        try
        {
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "desktop.runtimeconfig.json",
                CreateSingleFrameworkJson("Microsoft.NETCore.App", "10.0.0")
            );
            var linuxPackagePath = Path.Combine(temporaryDirectory, "linux.zip");
            await File.WriteAllTextAsync(linuxPackagePath, "linux-package");

            var manifest = await new AppPackageService().CreateAsync(
                temporaryDirectory,
                linuxPackagePath: linuxPackagePath,
                linuxPackageUrl: "https://example.com/linux.zip"
            );

            var artifact = manifest.Artifacts[AppPackageService.LinuxRuntimeIdentifier];
            Assert.AreEqual("https://example.com/linux.zip", artifact.Url);
            var expectedHash = Convert.ToHexStringLower(
                SHA256.HashData(await File.ReadAllBytesAsync(linuxPackagePath))
            );
            Assert.AreEqual(expectedHash, artifact.Sha256);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task CreateAsync_WithIncompleteLinuxArtifact_ThrowsArgumentException()
    {
        var temporaryDirectory = CreateProjectDirectory();
        try
        {
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "desktop.runtimeconfig.json",
                CreateSingleFrameworkJson("Microsoft.NETCore.App", "10.0.0")
            );

            await Assert.ThrowsAsync<ArgumentException>(
                () =>
                    new AppPackageService().CreateAsync(
                        temporaryDirectory,
                        linuxPackagePath: "package.zip"
                    )
            );
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    [DataRow("{", "无法解析运行时配置")]
    [DataRow("{ \"runtimeOptions\": {} }", "未找到 .NET 共享框架要求")]
    [DataRow(
        "{ \"runtimeOptions\": { \"framework\": { \"name\": \"Microsoft.NETCore.App\", \"version\": \"invalid\" } } }",
        "共享框架名称或版本无效"
    )]
    public async Task CreateAsync_InvalidRuntimeConfig_ThrowsInvalidDataException(
        string runtimeConfigContent,
        string expectedMessage
    )
    {
        var temporaryDirectory = CreateProjectDirectory();
        try
        {
            await WriteRuntimeConfigAsync(
                temporaryDirectory,
                "invalid.runtimeconfig.json",
                runtimeConfigContent
            );

            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => new AppPackageService().CreateAsync(temporaryDirectory)
            );

            StringAssert.Contains(exception.Message, expectedMessage);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Deserialize_LegacyManifestWithoutEnv_UsesEmptyEnvironment()
    {
        const string legacyManifest = """
            {
              "name": "UzonMail",
              "version": "0.23.0.0",
              "dependencies": {},
              "endpoint": "https://example.com/latest.json",
              "zipUrl": "https://example.com/package.zip",
              "ignores": [],
              "restartExecutablePath": "UzonMailDesktop.exe"
            }
            """;

        var manifest = JsonSerializer.Deserialize<AppPackage>(
            legacyManifest,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.IsNotNull(manifest);
        Assert.IsEmpty(manifest.Env);
    }

    private static string CreateProjectDirectory()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        File.Copy(
            typeof(AppPackageService).Assembly.Location,
            Path.Combine(temporaryDirectory, "UzonMailDesktop.exe")
        );
        return temporaryDirectory;
    }

    private static async Task WriteRuntimeConfigAsync(
        string rootDirectory,
        string relativePath,
        string content
    )
    {
        var runtimeConfigPath = Path.Combine(rootDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(runtimeConfigPath)!);
        await File.WriteAllTextAsync(runtimeConfigPath, content);
    }

    private static string CreateSingleFrameworkJson(string name, string version) =>
        $$"""
            {
              "runtimeOptions": {
                "framework": {
                  "name": "{{name}}",
                  "version": "{{version}}"
                }
              }
            }
            """;
}
