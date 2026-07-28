namespace UzonMailUpdater.Services;

/// <summary>
/// 按内容哈希获取更新依赖的本地文件
/// </summary>
public interface IDependencyDownloader
{
    Task<string> DownloadAsync(string hash, CancellationToken cancellationToken = default);
}
