using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing;
using UzonMailUpdater.Models;

namespace UzonMailUpdater.Services;

/// <summary>
/// 生成和写入应用更新清单
/// </summary>
public sealed class AppPackageService
{
    public const string ManifestFileName = "appPackage.json";
    public const string DefaultEndpoint = "https://uzonmail.uzoncloud.com/updates/latest.json";
    private const string DefaultZipUrlPrefix =
        "https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-";
    private static readonly string[] DefaultIgnores =
    [
        "Updater/**",
        "appsettings*.json",
        "service/data/**"
    ];

    /// <summary>
    /// 根据应用目录创建更新清单
    /// </summary>
    public async Task<AppPackage> CreateAsync(
        string projectDirectory,
        string? endpoint = null,
        string? zipUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        var rootDirectory = Path.GetFullPath(projectDirectory);
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"应用目录不存在：{rootDirectory}");

        var restartPath = "UzonMailDesktop.exe";
        var executablePath = GetSafeFilePath(rootDirectory, restartPath);
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("未找到桌面端可执行文件", executablePath);

        var version = FileVersionInfo.GetVersionInfo(executablePath).FileVersion;
        if (!Version.TryParse(version, out _))
            throw new InvalidOperationException($"无法读取有效的应用版本：{version}");

        var resolvedEndpoint = endpoint ?? DefaultEndpoint;
        var resolvedZipUrl = zipUrl ?? $"{DefaultZipUrlPrefix}{version}.zip";
        ValidateHttpUrl(resolvedEndpoint, nameof(endpoint));
        ValidateHttpUrl(resolvedZipUrl, nameof(zipUrl));

        var environment = RuntimeEnvironmentReader.Read(rootDirectory);
        var matcher = CreateIgnoreMatcher(DefaultIgnores.Append(ManifestFileName));
        var dependencies = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (
            var filePath in Directory.EnumerateFiles(
                rootDirectory,
                "*",
                SearchOption.AllDirectories
            )
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizeRelativePath(Path.GetRelativePath(rootDirectory, filePath));
            if (matcher.Match(relativePath).HasMatches)
                continue;

            dependencies.Add(relativePath, await ComputeSha256Async(filePath, cancellationToken));
        }

        return new AppPackage
        {
            Name = "UzonMail",
            Version = version,
            Env = environment,
            Dependencies = dependencies,
            Endpoint = resolvedEndpoint,
            ZipUrl = resolvedZipUrl,
            Ignores = [.. DefaultIgnores],
            RestartExecutablePath = restartPath
        };
    }

    /// <summary>
    /// 将清单写入默认位置和指定的发布位置
    /// </summary>
    public async Task WriteAsync(
        AppPackage manifest,
        string projectDirectory,
        IEnumerable<string> outputPaths,
        CancellationToken cancellationToken = default
    )
    {
        var outputs = outputPaths.Any()
            ? outputPaths
            : [Path.Combine(Path.GetFullPath(projectDirectory), ManifestFileName)];
        var content = JsonSerializer.Serialize(manifest, UpdaterJsonContext.Default.AppPackage);
        foreach (var outputPath in outputs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            var temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllTextAsync(temporaryPath, content, cancellationToken);
                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
    }

    internal static Matcher CreateIgnoreMatcher(IEnumerable<string> patterns)
    {
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (var pattern in patterns)
            matcher.AddInclude(pattern.Replace('\\', '/'));
        return matcher;
    }

    internal static string GetSafeFilePath(string rootDirectory, string relativePath)
    {
        var normalizedPath = NormalizeRelativePath(relativePath);
        var fullPath = Path.GetFullPath(Path.Combine(rootDirectory, normalizedPath));
        var rootWithSeparator = Path.EndsInDirectorySeparator(rootDirectory)
            ? rootDirectory
            : rootDirectory + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"文件路径超出应用目录：{relativePath}");
        return fullPath;
    }

    internal static string NormalizeRelativePath(string path)
    {
        var normalizedPath = path.Replace('\\', '/');
        if (
            Path.IsPathRooted(path)
            || normalizedPath.StartsWith("../", StringComparison.Ordinal)
            || normalizedPath.Contains("/../", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(normalizedPath)
        )
            throw new InvalidOperationException($"更新清单包含非法相对路径：{path}");
        return normalizedPath;
    }

    internal static async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken
    )
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash);
    }

    internal static void ValidateHttpUrl(string value, string parameterName)
    {
        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
        )
            throw new ArgumentException("必须是 HTTPS 绝对地址", parameterName);
    }
}
