using System.IO;
using System.Text.Json;
using UzonMailDesktop.Configuration;

namespace UzonMailDesktop.Services;

internal sealed record RequiredFramework(string Name, Version Version);

internal static class RuntimeConfigReader
{
    public static IReadOnlyList<RequiredFramework> Read(BackendOptions backend)
    {
        var executablePath = BackendProcessManager.ResolvePath(backend.ExecutablePath);
        var runtimeConfigPath = Path.ChangeExtension(executablePath, ".runtimeconfig.json");
        if (!File.Exists(runtimeConfigPath))
            throw new FileNotFoundException("无法读取后端运行时要求，runtimeconfig 文件不存在。", runtimeConfigPath);

        using var document = JsonDocument.Parse(File.ReadAllText(runtimeConfigPath));
        var runtimeOptions = document.RootElement.GetProperty("runtimeOptions");
        var result = new List<RequiredFramework>();
        if (runtimeOptions.TryGetProperty("frameworks", out var frameworks))
        {
            foreach (var framework in frameworks.EnumerateArray())
                AddFramework(framework, result);
        }
        else if (runtimeOptions.TryGetProperty("framework", out var framework))
        {
            AddFramework(framework, result);
        }

        return result;
    }

    private static void AddFramework(JsonElement framework, ICollection<RequiredFramework> result)
    {
        var name = framework.GetProperty("name").GetString();
        var versionText = framework.GetProperty("version").GetString();
        if (name is not null && Version.TryParse(versionText, out var version))
            result.Add(new RequiredFramework(name, version));
    }
}
