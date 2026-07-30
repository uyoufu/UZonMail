using System.Runtime.InteropServices;
using UzonMailDesktop.Localization;

namespace UzonMailDesktop.WebMessage.HostObjects;

/// <summary>
/// 向 WebView 页面提供桌面端语言读取和持久化能力
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class DesktopLocaleHostObject(IDesktopLocalizationService localization)
{
    /// <summary>
    /// 获取当前生效的桌面端语言代码
    /// </summary>
    public string GetCurrentLocale() => DesktopLocales.GetCultureName(localization.CurrentLocale);

    /// <summary>
    /// 验证并保存前端选择的语言代码
    /// </summary>
    public bool SetCurrentLocale(string locale) => localization.TrySetLocale(locale);
}
