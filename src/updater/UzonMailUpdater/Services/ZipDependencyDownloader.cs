using System.IO.Compression;
using System.Net.Http;
using UzonMailUpdater.Models;

namespace UzonMailUpdater.Services;

/// <summary>
/// 从完整 ZIP 包中提取按哈希索引的更新依赖
/// </summary>
internal sealed class ZipDependencyDownloader : IDependencyDownloader, IDisposable
{
    private readonly AppPackage _manifest;
    private readonly string _temporaryDirectory;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _extractionLock = new(1, 1);
    private readonly Dictionary<string, string> _filesByHash;
    private Task? _extractionTask;

    public ZipDependencyDownloader(
        AppPackage manifest,
        string temporaryDirectory,
        HttpClient httpClient
    )
    {
        _manifest = manifest;
        _temporaryDirectory = temporaryDirectory;
        _httpClient = httpClient;
        _filesByHash = manifest
            .Dependencies.GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 返回具有指定哈希的已校验依赖文件
    /// </summary>
    public async Task<string> DownloadAsync(
        string hash,
        CancellationToken cancellationToken = default
    )
    {
        if (!_filesByHash.TryGetValue(hash, out var relativePath))
            throw new InvalidOperationException($"更新包不包含依赖哈希：{hash}");

        await EnsureExtractedAsync(cancellationToken);
        var extractedPath = AppPackageService.GetSafeFilePath(
            Path.Combine(_temporaryDirectory, "content"),
            relativePath
        );
        if (!File.Exists(extractedPath))
            throw new InvalidDataException($"更新 ZIP 缺少文件：{relativePath}");

        var actualHash = await AppPackageService.ComputeSha256Async(
            extractedPath,
            cancellationToken
        );
        if (!string.Equals(hash, actualHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"更新文件校验失败：{relativePath}");
        return extractedPath;
    }

    private async Task EnsureExtractedAsync(CancellationToken cancellationToken)
    {
        if (_extractionTask is not null)
        {
            await _extractionTask.WaitAsync(cancellationToken);
            return;
        }

        await _extractionLock.WaitAsync(cancellationToken);
        try
        {
            _extractionTask ??= DownloadAndExtractAsync();
        }
        finally
        {
            _extractionLock.Release();
        }
        await _extractionTask.WaitAsync(cancellationToken);
    }

    private async Task DownloadAndExtractAsync()
    {
        var archivePath = Path.Combine(_temporaryDirectory, "update.zip");
        var contentDirectory = Path.Combine(_temporaryDirectory, "content");
        Directory.CreateDirectory(contentDirectory);
        using var response = await _httpClient.GetAsync(
            _manifest.ZipUrl,
            HttpCompletionOption.ResponseHeadersRead
        );
        response.EnsureSuccessStatusCode();
        await using (var source = await response.Content.ReadAsStreamAsync())
        await using (var target = File.Create(archivePath))
            await source.CopyToAsync(target);

        using var archive = ZipFile.OpenRead(archivePath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            var relativePath = AppPackageService.NormalizeRelativePath(entry.FullName);
            if (
                !_manifest.Dependencies.ContainsKey(relativePath)
                && !string.Equals(
                    relativePath,
                    AppPackageService.ManifestFileName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                continue;

            var destination = AppPackageService.GetSafeFilePath(contentDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, overwrite: true);
        }
    }

    public void Dispose() => _extractionLock.Dispose();
}
