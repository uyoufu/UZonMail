using System.Globalization;

namespace UzonMailDesktop.Localization;

/// <summary>
/// 桌面端当前支持的界面语言
/// </summary>
public enum DesktopLocale
{
    SimplifiedChinese,
    EnglishUnitedStates
}

internal static class DesktopLocales
{
    private const string SimplifiedChineseCultureName = "zh-CN";
    private const string EnglishUnitedStatesCultureName = "en-US";

    public static string GetCultureName(DesktopLocale locale) =>
        locale == DesktopLocale.EnglishUnitedStates
            ? EnglishUnitedStatesCultureName
            : SimplifiedChineseCultureName;

    public static bool TryParse(string? locale, out DesktopLocale desktopLocale)
    {
        if (string.Equals(locale, SimplifiedChineseCultureName, StringComparison.OrdinalIgnoreCase))
        {
            desktopLocale = DesktopLocale.SimplifiedChinese;
            return true;
        }

        if (
            string.Equals(
                locale,
                EnglishUnitedStatesCultureName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            desktopLocale = DesktopLocale.EnglishUnitedStates;
            return true;
        }

        desktopLocale = default;
        return false;
    }

    public static DesktopLocale Resolve(CultureInfo systemUiCulture, string? configuredLocale)
    {
        if (TryParse(configuredLocale, out var configured))
            return configured;

        if (TryParse(systemUiCulture.Name, out var exactMatch))
            return exactMatch;

        return systemUiCulture.TwoLetterISOLanguageName.Equals(
            "en",
            StringComparison.OrdinalIgnoreCase
        )
            ? DesktopLocale.EnglishUnitedStates
            : DesktopLocale.SimplifiedChinese;
    }
}
