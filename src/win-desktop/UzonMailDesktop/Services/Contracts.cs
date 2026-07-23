using System.Windows;

namespace UzonMailDesktop.Services;

public sealed record PrerequisiteItem(
    string Id,
    string Name,
    bool IsInstalled,
    Version? RequiredVersion = null
);

public sealed record InstallProgress(string Message, double Percentage);

public interface INavigationService
{
    event Action<object>? Navigated;
    void Navigate(object page);
}

public interface ISingleInstanceService : IDisposable
{
    bool TryAcquire();
}

public interface IPrerequisiteService
{
    Task<IReadOnlyList<PrerequisiteItem>> DetectAsync(
        CancellationToken cancellationToken = default
    );
    Task InstallAsync(
        IEnumerable<PrerequisiteItem> prerequisites,
        IProgress<InstallProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
}

public interface IBackendProcessManager : IDisposable
{
    bool OwnsProcess { get; }
    Task StartOrReuseAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}

public interface ITrayIconService : IDisposable
{
    void Start(Window window);
}

public interface IDialogService
{
    bool Confirm(string message, string title);
    void ShowError(string message, string title);
}
