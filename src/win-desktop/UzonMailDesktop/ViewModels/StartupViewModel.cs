using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using UzonMailDesktop.Configuration;
using UzonMailDesktop.Services;
using UzonMailDesktop.WebMessage.HostObjects;
using Application = System.Windows.Application;

namespace UzonMailDesktop.ViewModels;

public sealed partial class StartupViewModel : ObservableObject
{
    private readonly IPrerequisiteService _prerequisites;
    private readonly IBackendProcessManager _backend;
    private readonly INavigationService _navigation;
    private readonly BackendOptions _backendOptions;
    private readonly PrerequisiteOptions _prerequisiteOptions;
    private readonly IHostObjectRegistry _hostObjectRegistry;

    [ObservableProperty]
    private string statusMessage = "正在检测运行环境...";

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    private bool hasMissingPrerequisites;

    public ObservableCollection<PrerequisiteItem> MissingPrerequisites { get; } = [];

    public StartupViewModel(
        IPrerequisiteService prerequisites,
        IBackendProcessManager backend,
        INavigationService navigation,
        IHostObjectRegistry hostObjectRegistry,
        IOptions<BackendOptions> backendOptions,
        IOptions<PrerequisiteOptions> prerequisiteOptions
    )
    {
        _prerequisites = prerequisites;
        _backend = backend;
        _navigation = navigation;
        _hostObjectRegistry = hostObjectRegistry;
        _backendOptions = backendOptions.Value;
        _prerequisiteOptions = prerequisiteOptions.Value;
    }

    public Task InitializeAsync() => DetectAndContinueAsync();

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private Task RetryAsync() => DetectAndContinueAsync();

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallAsync()
    {
        IsBusy = true;
        try
        {
            var progress = new Progress<InstallProgress>(value =>
            {
                StatusMessage = value.Message;
                ProgressValue = value.Percentage;
            });
            await _prerequisites.InstallAsync(MissingPrerequisites, progress);
            await DetectAndContinueCoreAsync();
        }
        catch (OperationCanceledException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (Exception exception)
        {
            StatusMessage = $"安装失败：{exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenDownloadPage()
    {
        var url = MissingPrerequisites.Any(x => x.Id == PrerequisiteService.WebView2RuntimeId)
            ? "https://developer.microsoft.com/microsoft-edge/webview2/"
            : "https://dotnet.microsoft.com/download/dotnet/10.0";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    [RelayCommand]
    private static void Exit() => Application.Current.Shutdown();

    private async Task DetectAndContinueAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            await DetectAndContinueCoreAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = $"启动检查失败：{exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DetectAndContinueCoreAsync()
    {
        StatusMessage = "正在检测运行环境...";
        ProgressValue = 0;
        MissingPrerequisites.Clear();

        var detected = await _prerequisites.DetectAsync();
        foreach (var item in detected.Where(x => !x.IsInstalled))
            MissingPrerequisites.Add(item);

        HasMissingPrerequisites = MissingPrerequisites.Count > 0;
        if (HasMissingPrerequisites)
        {
            StatusMessage = "检测到缺失环境。确认后将从微软下载并安装以下组件。";
            if (!_prerequisiteOptions.ConfirmBeforeInstall)
                await InstallAsync();
            return;
        }

        StatusMessage = "环境检查通过，正在启动后端服务...";
        ProgressValue = 100;
        await _backend.StartOrReuseAsync();
        _navigation.Navigate(
            new BrowserViewModel(new Uri(_backendOptions.WebUrl), _hostObjectRegistry)
        );
    }

    private bool CanInstall() => !IsBusy && HasMissingPrerequisites;

    private bool CanRetry() => !IsBusy;
}
