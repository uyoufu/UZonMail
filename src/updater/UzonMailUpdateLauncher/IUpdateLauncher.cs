using System.Diagnostics;

namespace UzonMailUpdateLauncher;

/// <summary>
/// 启动独立更新器并将当前应用目录传递给它
/// </summary>
public interface IUpdateLauncher
{
    bool TryStart(string applicationDirectory, int parentProcessId);
}

/// <summary>
/// 使用安装目录中的更新器启动更新流程
/// </summary>
public sealed class UpdateLauncher : IUpdateLauncher
{
    private const string UpdaterRelativePath = "Updater/UzonMailUpdater.exe";

    public bool TryStart(string applicationDirectory, int parentProcessId)
    {
        var rootDirectory = Path.GetFullPath(applicationDirectory);
        var updaterPath = Path.Combine(rootDirectory, UpdaterRelativePath);
        if (!File.Exists(updaterPath))
            return false;

        var startInfo = new ProcessStartInfo(updaterPath)
        {
            WorkingDirectory = rootDirectory,
            UseShellExecute = true
        };
        startInfo.ArgumentList.Add("update");
        startInfo.ArgumentList.Add("--project-directory");
        startInfo.ArgumentList.Add(rootDirectory);
        startInfo.ArgumentList.Add("--parent-process-id");
        startInfo.ArgumentList.Add(
            parentProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );
        return Process.Start(startInfo) is not null;
    }
}
