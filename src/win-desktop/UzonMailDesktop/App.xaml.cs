using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UzonMailDesktop.Configuration;
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

        try
        {
            try
            {
                UpdaterBootstrapService.InstallPendingUpdater(AppContext.BaseDirectory);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"更新器更新失败，将在下次启动时重试：{exception.Message}",
                    "更新器提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            _host = BuildHost(e.Args);
            await _host.StartAsync();

            var singleInstance = _host.Services.GetRequiredService<ISingleInstanceService>();
            if (!singleInstance.TryAcquire())
            {
                MessageBox.Show("不能重复运行", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show(exception.Message, "启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
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

        builder
            .Services.AddOptions<BackendOptions>()
            .Bind(builder.Configuration.GetSection(BackendOptions.SectionName))
            .Validate(
                x => !string.IsNullOrWhiteSpace(x.ExecutablePath),
                "Backend:ExecutablePath 不能为空。"
            )
            .Validate(
                x => Uri.TryCreate(x.WebUrl, UriKind.Absolute, out _),
                "Backend:WebUrl 必须是绝对 URL。"
            )
            .Validate(
                x => Uri.TryCreate(x.ReadinessUrl, UriKind.Absolute, out _),
                "Backend:ReadinessUrl 必须是绝对 URL。"
            )
            .Validate(
                x => x.StartupTimeoutSeconds is >= 1 and <= 600,
                "Backend:StartupTimeoutSeconds 必须介于 1 和 600 之间。"
            )
            .ValidateOnStart();
        builder
            .Services.AddOptions<PrerequisiteOptions>()
            .Bind(builder.Configuration.GetSection(PrerequisiteOptions.SectionName))
            .Validate(
                x => Uri.TryCreate(x.DotNetReleaseMetadataBaseUrl, UriKind.Absolute, out _),
                "Prerequisites:DotNetReleaseMetadataBaseUrl 必须是绝对 URL。"
            )
            .Validate(
                x => Uri.TryCreate(x.WebView2BootstrapperUrl, UriKind.Absolute, out _),
                "Prerequisites:WebView2BootstrapperUrl 必须是绝对 URL。"
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
