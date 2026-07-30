namespace UzonMailDesktop.Localization;

/// <summary>
/// 提供桌面端文本翻译和语言切换能力
/// </summary>
public interface IDesktopLocalizationService
{
    /// <summary>
    /// 获取当前生效的桌面端语言
    /// </summary>
    DesktopLocale CurrentLocale { get; }

    /// <summary>
    /// 当桌面端语言切换完成后触发
    /// </summary>
    event EventHandler? LocaleChanged;

    /// <summary>
    /// 获取指定资源键在当前语言下的文本
    /// </summary>
    string GetText(DesktopTextKey key, params object?[] formatArguments);

    /// <summary>
    /// 验证并持久化来自前端的语言代码
    /// </summary>
    bool TrySetLocale(string locale);
}
