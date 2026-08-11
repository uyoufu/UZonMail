using System.Text.Json.Serialization;

namespace UzonMailUpdater.Models;

/// <summary>
/// 描述一个可下载和安装的应用版本
/// </summary>
public sealed class AppPackage
{
    public required string Name { get; init; }
    public required string Version { get; init; }

    /// <summary>
    /// 安装应用所需的 .NET 共享框架及其最低版本
    /// </summary>
    public Dictionary<string, string> Env { get; init; } = [];
    public required Dictionary<string, string> Dependencies { get; init; }
    public required string Endpoint { get; init; }
    public required string ZipUrl { get; init; }
    public required List<string> Ignores { get; init; }
    public required string RestartExecutablePath { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
)]
[JsonSerializable(typeof(AppPackage))]
internal sealed partial class UpdaterJsonContext : JsonSerializerContext;
