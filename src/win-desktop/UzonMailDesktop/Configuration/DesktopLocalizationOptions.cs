namespace UzonMailDesktop.Configuration;

/// <summary>
/// 桌面端界面语言配置
/// </summary>
public sealed class DesktopLocalizationOptions
{
    public const string SectionName = "Localization";

    public string? Locale { get; init; }
}
