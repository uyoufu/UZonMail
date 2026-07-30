using System.Globalization;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UzonMailDesktop.Configuration;
using UzonMailDesktop.Localization;
using UzonMailDesktop.Services;
using UzonMailDesktop.ViewModels;
using UzonMailDesktop.WebMessage.HostObjects;
using UzonMailUpdater.Launcher;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace UzonMailDesktop;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IDesktopLocalizationService? localization = null;
        try
        {
            _host = BuildHost(e.Args);
            localization = _host.Services.GetRequiredService<IDesktopLocalizationService>();

            try
            {
                UpdaterBootstrapService.InstallPendingUpdater(
                    AppContext.BaseDirectory,
                    localization
                );
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    localization.GetText(
                        DesktopTextKey.DialogUpdaterInstallFailed,
                        exception.Message
                    ),
                    localization.GetText(DesktopTextKey.DialogUpdaterTitle),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            await _host.StartAsync();

            var singleInstance = _host.Services.GetRequiredService<ISingleInstanceService>();
            if (!singleInstance.TryAcquire())
            {
                MessageBox.Show(
                    localization.GetText(DesktopTextKey.DialogSingleInstanceMessage),
                    localization.GetText(DesktopTextKey.DialogSingleInstanceTitle),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Shutdown();
                return;
            }

            var window = _host.Services.GetRequiredService<MainWindow>();
            var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
            var navigation = _host.Services.GetRequiredService<INavigationService>();
            var tray = _host.Services.GetRequiredService<ITrayIconService>();

            window.DataContext = viewModel;
            navigation.Navigated += page =>
            {
                if (page is BrowserViewModel)
                    tray.Start(window);
            };

            MainWindow = window;
            window.Show();
            await _host.Services.GetRequiredService<StartupViewModel>().InitializeAsync();
        }
        catch (Exception exception)
        {
            var title =
                localization?.GetText(DesktopTextKey.DialogStartupFailedTitle) ?? "Startup failed";
            MessageBox.Show(exception.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            _host.Services.GetService<ITrayIconService>()?.Dispose();
            _host
                .Services.GetService<IBackendProcessManager>()
                ?.StopAsync()
                .GetAwaiter()
                .GetResult();
            _host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static IHost BuildHost(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = environment
            }
        );

        builder
            .Configuration.SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("UZONMAIL_DESKTOP_");

        var configuredLocale = builder
            .Configuration.GetSection(DesktopLocalizationOptions.SectionName)
            .Get<DesktopLocalizationOptions>()
            ?.Locale;
        var localization = new DesktopLocalizationService(
            new DesktopLocalizationSettingsStore(),
            CultureInfo.CurrentUICulture,
            configuredLocale
        );
        builder.Services.AddSingleton<IDesktopLocalizationService>(localization);

        builder
            .Services.AddOptions<BackendOptions>()
            .Bind(builder.Configuration.GetSection(BackendOptions.SectionName))
            .Validate(
                x => !string.IsNullOrWhiteSpace(x.ExecutablePath),
                localization.GetText(DesktopTextKey.BackendExecutablePathInvalid)
            )
            .Validate(
                x => Uri.TryCreate(x.WebUrl, UriKind.Absolute, out _),
                localization.GetText(DesktopTextKey.BackendWebUrlInvalid)
            )
            .Validate(
                x => Uri.TryCreate(x.ReadinessUrl, UriKind.Absolute, out _),
                localization.GetText(DesktopTextKey.BackendReadinessUrlInvalid)
            )
            .Validate(
                x => x.StartupTimeoutSeconds is >= 1 and <= 600,
                localization.GetText(DesktopTextKey.BackendStartupTimeoutInvalid)
            )
            .ValidateOnStart();
        builder
            .Services.AddOptions<PrerequisiteOptions>()
            .Bind(builder.Configuration.GetSection(PrerequisiteOptions.SectionName))
            .Validate(
                x => Uri.TryCreate(x.DotNetReleaseMetadataBaseUrl, UriKind.Absolute, out _),
                localization.GetText(DesktopTextKey.PrerequisiteMetadataUrlInvalid)
            )
            .Validate(
                x => Uri.TryCreate(x.WebView2BootstrapperUrl, UriKind.Absolute, out _),
                localization.GetText(DesktopTextKey.PrerequisiteWebViewUrlInvalid)
            )
            .ValidateOnStart();

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddSingleton<StartupViewModel>();
        builder.Services.AddTransient<BrowserViewModel>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ISingleInstanceService, SingleInstanceService>();
        builder.Services.AddSingleton<IPrerequisiteService, PrerequisiteService>();
        builder.Services.AddSingleton<IBackendProcessManager, BackendProcessManager>();
        builder.Services.AddSingleton<ITrayIconService, TrayIconService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IUpdateLauncher, UpdateLauncher>();
        builder.Services.AddSingleton<IHostObjectRegistry, HostObjectRegistry>();

        return builder.Build();
    }
}
