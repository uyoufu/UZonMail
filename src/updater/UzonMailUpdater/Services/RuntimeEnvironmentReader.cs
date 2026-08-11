using System.Text.Json;

namespace UzonMailUpdater.Services;

/// <summary>
/// 从发布产物的 runtimeconfig 文件汇总安装所需的 .NET 共享框架
/// </summary>
internal static class RuntimeEnvironmentReader
{
    private const string RuntimeConfigSearchPattern = "*.runtimeconfig.json";
    private const string RuntimeOptionsPropertyName = "runtimeOptions";
    private const string FrameworkPropertyName = "framework";
    private const string FrameworksPropertyName = "frameworks";
    private const string FrameworkNamePropertyName = "name";
    private const string FrameworkVersionPropertyName = "version";

    /// <summary>
    /// 递归读取发布目录中的共享框架要求；不读取 includedFrameworks，因为它们已包含在自包含产物中
    /// </summary>
    public static Dictionary<string, string> Read(string rootDirectory)
    {
        var runtimeConfigPaths = Directory
            .EnumerateFiles(rootDirectory, RuntimeConfigSearchPattern, SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (runtimeConfigPaths.Length == 0)
            throw new InvalidDataException("发布目录中未找到 runtimeconfig 文件，无法推断安装环境");

        var requirements = new Dictionary<string, (Version Version, string SourcePath)>(
            StringComparer.Ordinal
        );
        foreach (var runtimeConfigPath in runtimeConfigPaths)
        {
            using var document = ParseRuntimeConfig(runtimeConfigPath);
            if (
                document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(
                    RuntimeOptionsPropertyName,
                    out var runtimeOptions
                )
                || runtimeOptions.ValueKind != JsonValueKind.Object
            )
                throw new InvalidDataException(
                    $"运行时配置缺少 {RuntimeOptionsPropertyName} 对象：{runtimeConfigPath}"
                );

            if (runtimeOptions.TryGetProperty(FrameworkPropertyName, out var framework))
                AddRequirement(framework, runtimeConfigPath, requirements);

            if (!runtimeOptions.TryGetProperty(FrameworksPropertyName, out var frameworks))
                continue;
            if (frameworks.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException(
                    $"运行时配置的 {FrameworksPropertyName} 必须是数组：{runtimeConfigPath}"
                );
            foreach (var frameworkElement in frameworks.EnumerateArray())
                AddRequirement(frameworkElement, runtimeConfigPath, requirements);
        }

        if (requirements.Count == 0)
            throw new InvalidDataException("runtimeconfig 中未找到 .NET 共享框架要求");

        return requirements
            .OrderBy(requirement => requirement.Key, StringComparer.Ordinal)
            .ToDictionary(
                requirement => requirement.Key,
                requirement => requirement.Value.Version.ToString(),
                StringComparer.Ordinal
            );
    }

    private static JsonDocument ParseRuntimeConfig(string runtimeConfigPath)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllText(runtimeConfigPath));
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"无法解析运行时配置：{runtimeConfigPath}", exception);
        }
    }

    private static void AddRequirement(
        JsonElement framework,
        string runtimeConfigPath,
        IDictionary<string, (Version Version, string SourcePath)> requirements
    )
    {
        if (framework.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException($"共享框架配置必须是对象：{runtimeConfigPath}");

        var frameworkName =
            framework.TryGetProperty(FrameworkNamePropertyName, out var nameElement)
            && nameElement.ValueKind == JsonValueKind.String
                ? nameElement.GetString()
                : null;
        var versionText =
            framework.TryGetProperty(FrameworkVersionPropertyName, out var versionElement)
            && versionElement.ValueKind == JsonValueKind.String
                ? versionElement.GetString()
                : null;
        if (
            string.IsNullOrWhiteSpace(frameworkName)
            || !Version.TryParse(versionText, out var version)
        )
            throw new InvalidDataException($"共享框架名称或版本无效：{runtimeConfigPath}");

        if (!requirements.TryGetValue(frameworkName, out var existingRequirement))
        {
            requirements.Add(frameworkName, (version, runtimeConfigPath));
            return;
        }

        if (existingRequirement.Version != version)
            throw new InvalidDataException(
                $"共享框架 {frameworkName} 存在版本冲突："
                    + $"{existingRequirement.Version} ({existingRequirement.SourcePath}) / "
                    + $"{version} ({runtimeConfigPath})"
            );
    }
}
