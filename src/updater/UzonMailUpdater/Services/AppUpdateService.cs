using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing;
using UzonMailUpdater.Models;

namespace UzonMailUpdater.Services;

/// <summary>
/// 协调完整的应用更新、恢复和重启流程
/// </summary>
public sealed class AppUpdateService
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(15) };

    /// <summary>
    /// 下载、校验并安装最新应用版本
    /// </summary>
    public async Task UpdateAsync(
        string projectDirectory,
        int? parentProcessId,
        Action<string>? reportProgress = null,
        CancellationToken cancellationToken = default
    )
    {
        var rootDirectory = Path.GetFullPath(projectDirectory);
        var localManifestPath = Path.Combine(rootDirectory, AppPackageService.ManifestFileName);
        var localManifest = await ReadManifestAsync(localManifestPath, cancellationToken);
        ValidateManifest(localManifest);

        reportProgress?.Invoke("[cyan]正在获取最新版本信息[/]");
        var latestManifest = await DownloadManifestAsync(localManifest.Endpoint, cancellationToken);
        ValidateManifest(latestManifest);
        if (!string.Equals(localManifest.Name, latestManifest.Name, StringComparison.Ordinal))
            throw new InvalidDataException("更新清单的软件名称不匹配");
        if (Version.Parse(latestManifest.Version) <= Version.Parse(localManifest.Version))
            throw new InvalidOperationException("当前已经是最新版本");

        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "UzonMailUpdater",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            reportProgress?.Invoke("[cyan]正在下载并校验更新包[/]");
            using var downloader = new ZipDependencyDownloader(
                latestManifest,
                temporaryDirectory,
                _httpClient
            );
            var hashes = latestManifest
                .Dependencies.Values.Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var downloadedFiles = await Task.WhenAll(
                hashes.Select(async hash =>
                    (Hash: hash, Path: await downloader.DownloadAsync(hash, cancellationToken))
                )
            );
            var filesByHash = downloadedFiles.ToDictionary(
                x => x.Hash,
                x => x.Path,
                StringComparer.OrdinalIgnoreCase
            );

            await WaitForParentProcessAsync(parentProcessId, cancellationToken);
            reportProgress?.Invoke("[cyan]正在替换应用文件[/]");
            await ApplyAsync(
                rootDirectory,
                localManifest,
                latestManifest,
                filesByHash,
                temporaryDirectory,
                cancellationToken
            );

            reportProgress?.Invoke("[cyan]正在重启应用[/]");
            StartApplication(rootDirectory, latestManifest.RestartExecutablePath);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private async Task ApplyAsync(
        string rootDirectory,
        AppPackage localManifest,
        AppPackage latestManifest,
        IReadOnlyDictionary<string, string> filesByHash,
        string temporaryDirectory,
        CancellationToken cancellationToken
    )
    {
        var backupDirectory = Path.Combine(temporaryDirectory, "backup");
        var createdPaths = new List<string>();
        var protectedMatcher = AppPackageService.CreateIgnoreMatcher(
            localManifest
                .Ignores.Concat(latestManifest.Ignores)
                .Append(AppPackageService.ManifestFileName)
        );
        try
        {
            foreach (var (relativePath, hash) in latestManifest.Dependencies)
            {
                if (protectedMatcher.Match(relativePath).HasMatches)
                    continue;

                var destination = AppPackageService.GetSafeFilePath(rootDirectory, relativePath);
                var backupPath = AppPackageService.GetSafeFilePath(backupDirectory, relativePath);
                if (File.Exists(destination))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    File.Move(destination, backupPath, overwrite: true);
                }
                else
                {
                    createdPaths.Add(destination);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(filesByHash[hash], destination, overwrite: true);
            }

            foreach (
                var stalePath in localManifest.Dependencies.Keys.Except(
                    latestManifest.Dependencies.Keys,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            {
                if (protectedMatcher.Match(stalePath).HasMatches)
                    continue;

                var destination = AppPackageService.GetSafeFilePath(rootDirectory, stalePath);
                if (!File.Exists(destination))
                    continue;

                var backupPath = AppPackageService.GetSafeFilePath(backupDirectory, stalePath);
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                File.Move(destination, backupPath, overwrite: true);
            }

            var packageService = new AppPackageService();
            await packageService.WriteAsync(latestManifest, rootDirectory, [], cancellationToken);
        }
        catch
        {
            foreach (var createdPath in createdPaths.Where(File.Exists))
                File.Delete(createdPath);
            if (Directory.Exists(backupDirectory))
            {
                foreach (
                    var backupPath in Directory.EnumerateFiles(
                        backupDirectory,
                        "*",
                        SearchOption.AllDirectories
                    )
                )
                {
                    var relativePath = Path.GetRelativePath(backupDirectory, backupPath);
                    var destination = AppPackageService.GetSafeFilePath(
                        rootDirectory,
                        relativePath
                    );
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    File.Move(backupPath, destination, overwrite: true);
                }
            }
            throw;
        }
    }

    private async Task<AppPackage> DownloadManifestAsync(
        string endpoint,
        CancellationToken cancellationToken
    )
    {
        AppPackageService.ValidateHttpUrl(endpoint, nameof(endpoint));
        await using var stream = await _httpClient.GetStreamAsync(endpoint, cancellationToken);
        return await JsonSerializer.DeserializeAsync(
                stream,
                UpdaterJsonContext.Default.AppPackage,
                cancellationToken
            ) ?? throw new InvalidDataException("远端更新清单为空");
    }

    private static async Task<AppPackage> ReadManifestAsync(
        string manifestPath,
        CancellationToken cancellationToken
    )
    {
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("未找到本地更新清单", manifestPath);
        await using var stream = File.OpenRead(manifestPath);
        return await JsonSerializer.DeserializeAsync(
                stream,
                UpdaterJsonContext.Default.AppPackage,
                cancellationToken
            ) ?? throw new InvalidDataException("本地更新清单为空");
    }

    private static void ValidateManifest(AppPackage manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Name) || !Version.TryParse(manifest.Version, out _))
            throw new InvalidDataException("更新清单缺少有效的软件名称或版本");
        if (
            manifest.Env is null
            || manifest.Env.Any(requirement =>
                string.IsNullOrWhiteSpace(requirement.Key)
                || !Version.TryParse(requirement.Value, out _)
            )
        )
            throw new InvalidDataException("更新清单包含无效的运行环境要求");
        AppPackageService.ValidateHttpUrl(manifest.Endpoint, nameof(manifest.Endpoint));
        AppPackageService.ValidateHttpUrl(manifest.ZipUrl, nameof(manifest.ZipUrl));
        AppPackageService.NormalizeRelativePath(manifest.RestartExecutablePath);
        foreach (var (relativePath, hash) in manifest.Dependencies)
        {
            AppPackageService.NormalizeRelativePath(relativePath);
            if (hash.Length != 64 || !hash.All(Uri.IsHexDigit))
                throw new InvalidDataException($"更新清单包含无效哈希：{relativePath}");
        }
    }

    private static async Task WaitForParentProcessAsync(
        int? parentProcessId,
        CancellationToken cancellationToken
    )
    {
        if (parentProcessId is not > 0)
            return;
        try
        {
            using var process = Process.GetProcessById(parentProcessId.Value);
            await process
                .WaitForExitAsync(cancellationToken)
                .WaitAsync(TimeSpan.FromMinutes(2), cancellationToken);
        }
        catch (ArgumentException)
        {
            // 启动器创建更新进程后，父进程可能已经结束
        }
    }

    private static void StartApplication(string rootDirectory, string restartExecutablePath)
    {
        var executablePath = AppPackageService.GetSafeFilePath(
            rootDirectory,
            restartExecutablePath
        );
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("更新后未找到重启程序", executablePath);
        Process.Start(
            new ProcessStartInfo(executablePath)
            {
                WorkingDirectory = rootDirectory,
                UseShellExecute = true
            }
        );
    }
}
