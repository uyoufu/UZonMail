using System.Runtime.InteropServices;
using UzonMailUpdater.Launcher;
using Application = System.Windows.Application;

namespace UzonMailDesktop.WebMessage.HostObjects;

/// <summary>
/// 向 WebView 页面提供桌面端更新启动能力
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class DesktopUpdateHostObject(IUpdateLauncher updateLauncher)
{
    /// <summary>
    /// 启动独立更新器并关闭当前桌面端
    /// </summary>
    public bool BeginUpdate()
    {
        if (!updateLauncher.TryStart(AppContext.BaseDirectory, Environment.ProcessId))
            return false;

        // 更新器必须在当前进程释放 WebView 和服务文件后才可替换应用目录
        Application.Current.Dispatcher.BeginInvoke(
            new Action(() => Application.Current.Shutdown())
        );
        return true;
    }
}
