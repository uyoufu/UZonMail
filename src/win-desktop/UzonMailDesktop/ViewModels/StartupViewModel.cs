using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using UzonMailDesktop.Configuration;
using UzonMailDesktop.Localization;
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
    private readonly IDesktopLocalizationService _localization;
    private DesktopTextKey? _statusTextKey;
    private object?[] _statusFormatArguments = [];

    [ObservableProperty]
    private string statusMessage = string.Empty;

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

    public string StartupTitle => _localization.GetText(DesktopTextKey.StartupTitle);

    public string StartupSubtitle => _localization.GetText(DesktopTextKey.StartupSubtitle);

    public string RetryButtonText => _localization.GetText(DesktopTextKey.ButtonRetry);

    public string OpenDownloadPageButtonText =>
        _localization.GetText(DesktopTextKey.ButtonOpenDownloadPage);

    public string InstallAndContinueButtonText =>
        _localization.GetText(DesktopTextKey.ButtonInstallAndContinue);

    public string ExitButtonText => _localization.GetText(DesktopTextKey.ButtonExit);

    public bool IsProgressIndeterminate => IsBusy && ProgressValue <= 0;

    public StartupViewModel(
        IPrerequisiteService prerequisites,
        IBackendProcessManager backend,
        INavigationService navigation,
        IHostObjectRegistry hostObjectRegistry,
        IOptions<BackendOptions> backendOptions,
        IOptions<PrerequisiteOptions> prerequisiteOptions,
        IDesktopLocalizationService localization
    )
    {
        _prerequisites = prerequisites;
        _backend = backend;
        _navigation = navigation;
        _hostObjectRegistry = hostObjectRegistry;
        _backendOptions = backendOptions.Value;
        _prerequisiteOptions = prerequisiteOptions.Value;
        _localization = localization;
        _localization.LocaleChanged += OnLocaleChanged;
        SetStatus(DesktopTextKey.StartupCheckingEnvironment);
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
                SetStatusMessage(value.Message);
                ProgressValue = value.Percentage;
            });
            await _prerequisites.InstallAsync(MissingPrerequisites, progress);
            await DetectAndContinueCoreAsync();
        }
        catch (OperationCanceledException exception)
        {
            SetStatusMessage(exception.Message);
        }
        catch (Exception exception)
        {
            SetStatus(DesktopTextKey.StartupInstallFailed, exception.Message);
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
            SetStatus(DesktopTextKey.StartupCheckFailed, exception.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DetectAndContinueCoreAsync()
    {
        SetStatus(DesktopTextKey.StartupCheckingEnvironment);
        ProgressValue = 0;
        MissingPrerequisites.Clear();

        var detected = await _prerequisites.DetectAsync();
        foreach (var item in detected.Where(x => !x.IsInstalled))
            MissingPrerequisites.Add(item);

        HasMissingPrerequisites = MissingPrerequisites.Count > 0;
        if (HasMissingPrerequisites)
        {
            SetStatus(DesktopTextKey.StartupMissingPrerequisites);
            if (!_prerequisiteOptions.ConfirmBeforeInstall)
                await InstallAsync();
            return;
        }

        SetStatus(DesktopTextKey.StartupStartingBackend);
        ProgressValue = 100;
        await _backend.StartOrReuseAsync();
        _navigation.Navigate(
            new BrowserViewModel(
                new Uri(_backendOptions.WebUrl),
                _hostObjectRegistry,
                _localization
            )
        );
    }

    private bool CanInstall() => !IsBusy && HasMissingPrerequisites;

    private bool CanRetry() => !IsBusy;

    partial void OnProgressValueChanged(double value) =>
        OnPropertyChanged(nameof(IsProgressIndeterminate));

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsProgressIndeterminate));

    private void SetStatus(DesktopTextKey key, params object?[] formatArguments)
    {
        _statusTextKey = key;
        _statusFormatArguments = formatArguments;
        StatusMessage = _localization.GetText(key, formatArguments);
    }

    private void SetStatusMessage(string message)
    {
        _statusTextKey = null;
        _statusFormatArguments = [];
        StatusMessage = message;
    }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (_statusTextKey is { } statusTextKey)
                StatusMessage = _localization.GetText(statusTextKey, _statusFormatArguments);

            OnPropertyChanged(nameof(StartupTitle));
            OnPropertyChanged(nameof(StartupSubtitle));
            OnPropertyChanged(nameof(RetryButtonText));
            OnPropertyChanged(nameof(OpenDownloadPageButtonText));
            OnPropertyChanged(nameof(InstallAndContinueButtonText));
            OnPropertyChanged(nameof(ExitButtonText));
        });
    }
}
