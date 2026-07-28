using Microsoft.Web.WebView2.Core;
using UzonMailUpdater.Launcher;

namespace UzonMailDesktop.WebMessage.HostObjects;

/// <summary>
/// 统一注册 WebView 可调用的桌面端宿主对象
/// </summary>
public interface IHostObjectRegistry
{
    void Register(CoreWebView2 webView);
}

/// <summary>
/// 管理全部宿主对象的注册名称和实例生命周期
/// </summary>
internal sealed class HostObjectRegistry(IUpdateLauncher updateLauncher) : IHostObjectRegistry
{
    public const string DesktopUpdaterObjectName = "uzonMailUpdater";

    public void Register(CoreWebView2 webView)
    {
        webView.AddHostObjectToScript(
            DesktopUpdaterObjectName,
            new DesktopUpdateHostObject(updateLauncher)
        );
    }
}
