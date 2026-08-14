namespace UzonMailUpdater.Models;

/// <summary>
/// 描述特定运行时平台可下载发布包的位置和内容摘要
/// </summary>
public sealed class ReleaseArtifact
{
    public required string Url { get; init; }
    public required string Sha256 { get; init; }
}
