using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using UzonMailDesktop.Configuration;

namespace UzonMailDesktop.Localization;

internal sealed class DesktopLocalizationSettingsStore
{
    private const string ConfigurationFileName = "appsettings.Production.json";
    private readonly string _configurationFilePath;

    public DesktopLocalizationSettingsStore()
        : this(Path.Combine(AppContext.BaseDirectory, ConfigurationFileName)) { }

    internal DesktopLocalizationSettingsStore(string configurationFilePath)
    {
        _configurationFilePath = configurationFilePath;
    }

    public void WriteLocale(string locale)
    {
        var directory = Path.GetDirectoryName(_configurationFilePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException(
                "Desktop configuration directory is not available."
            );

        Directory.CreateDirectory(directory);
        var configuration = ReadConfiguration();
        if (configuration[DesktopLocalizationOptions.SectionName] is not JsonObject localization)
        {
            if (configuration.ContainsKey(DesktopLocalizationOptions.SectionName))
                throw new JsonException("Desktop localization configuration must be an object.");

            localization = new JsonObject();
            configuration[DesktopLocalizationOptions.SectionName] = localization;
        }

        localization[nameof(DesktopLocalizationOptions.Locale)] = locale;

        // 与目标文件同目录写入后再替换，避免进程中断时留下不完整的生产配置
        var temporaryFilePath = $"{_configurationFilePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var json = configuration.ToJsonString(
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(temporaryFilePath, json);
            File.Move(temporaryFilePath, _configurationFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
                File.Delete(temporaryFilePath);
        }
    }

    private JsonObject ReadConfiguration()
    {
        if (!File.Exists(_configurationFilePath))
            return [];

        return JsonNode.Parse(File.ReadAllText(_configurationFilePath)) as JsonObject
            ?? throw new JsonException("Desktop production configuration must be a JSON object.");
    }
}
