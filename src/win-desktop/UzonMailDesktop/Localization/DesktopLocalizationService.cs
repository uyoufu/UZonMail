using System.Globalization;
using System.IO;
using System.Resources;

namespace UzonMailDesktop.Localization;

internal sealed class DesktopLocalizationService : IDesktopLocalizationService
{
    private const string ResourceBaseName = "UzonMailDesktop.Localization.DesktopStrings";
    private readonly object _localeLock = new();
    private readonly DesktopUserSettingsStore _settingsStore;
    private readonly ResourceManager _resourceManager =
        new(ResourceBaseName, typeof(DesktopLocalizationService).Assembly);
    private DesktopLocale _currentLocale;

    public DesktopLocalizationService()
        : this(new DesktopUserSettingsStore(), CultureInfo.CurrentUICulture) { }

    internal DesktopLocalizationService(
        DesktopUserSettingsStore settingsStore,
        CultureInfo systemUiCulture
    )
    {
        _settingsStore = settingsStore;
        _currentLocale = DesktopLocales.Resolve(systemUiCulture, settingsStore.ReadLocale());
        ApplyDefaultCulture(_currentLocale);
    }

    public DesktopLocale CurrentLocale
    {
        get
        {
            lock (_localeLock)
                return _currentLocale;
        }
    }

    public event EventHandler? LocaleChanged;

    public string GetText(DesktopTextKey key, params object?[] formatArguments)
    {
        var culture = CultureInfo.GetCultureInfo(DesktopLocales.GetCultureName(CurrentLocale));
        var text = _resourceManager.GetString(key.ToString(), culture) ?? key.ToString();
        return formatArguments.Length == 0 ? text : string.Format(culture, text, formatArguments);
    }

    public bool TrySetLocale(string locale)
    {
        if (!DesktopLocales.TryParse(locale, out var nextLocale))
            return false;

        try
        {
            _settingsStore.WriteLocale(DesktopLocales.GetCultureName(nextLocale));
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        var hasChanged = false;
        lock (_localeLock)
        {
            if (_currentLocale != nextLocale)
            {
                _currentLocale = nextLocale;
                hasChanged = true;
            }
        }

        ApplyDefaultCulture(nextLocale);
        if (hasChanged)
            LocaleChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    private static void ApplyDefaultCulture(DesktopLocale locale)
    {
        var culture = CultureInfo.GetCultureInfo(DesktopLocales.GetCultureName(locale));
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
