using System.Diagnostics;

namespace UzonMailUpdater.Launcher;

/// <summary>
/// 启动独立更新器并将当前应用目录传递给它
/// </summary>
public interface IUpdateLauncher
{
    bool TryStart(string applicationDirectory, int parentProcessId);
}
