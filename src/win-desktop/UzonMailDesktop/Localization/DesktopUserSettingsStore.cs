using System.IO;
using System.Text.Json;

namespace UzonMailDesktop.Localization;

internal sealed class DesktopUserSettingsStore
{
    private const string SettingsFileName = "desktop-settings.json";
    private readonly string _settingsFilePath;

    public DesktopUserSettingsStore()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "UzonMail",
                SettingsFileName
            )
        ) { }

    internal DesktopUserSettingsStore(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
    }

    public string? ReadLocale()
    {
        if (!File.Exists(_settingsFilePath))
            return null;

        try
        {
            return JsonSerializer
                .Deserialize<DesktopUserSettings>(File.ReadAllText(_settingsFilePath))
                ?.Locale;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void WriteLocale(string locale)
    {
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("Desktop settings directory is not available.");

        Directory.CreateDirectory(directory);
        var temporaryFilePath = $"{_settingsFilePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var json = JsonSerializer.Serialize(new DesktopUserSettings(locale));
            File.WriteAllText(temporaryFilePath, json);
            File.Move(temporaryFilePath, _settingsFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
                File.Delete(temporaryFilePath);
        }
    }

    private sealed record DesktopUserSettings(string Locale);
}
