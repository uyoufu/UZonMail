using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace UzonMailDesktop.Services;

internal sealed class TrayIconService(IDialogService dialogs, ILogger<TrayIconService> logger)
    : ITrayIconService
{
    private NotifyIcon? _notifyIcon;
    private Window? _window;
    private bool _exiting;

    public void Start(Window window)
    {
        if (_notifyIcon is not null)
            return;

        _window = window;
        _window.Closing += OnWindowClosing;

        var menu = new ContextMenuStrip();
        menu.Items.Add("打开", null, (_, _) => ShowWindow());
        menu.Items.Add("退出", null, (_, _) => Exit());

        using var iconStream =
            Application
                .GetResourceStream(new Uri("pack://application:,,,/Resource/uzon-mail.ico"))
                ?.Stream ?? throw new InvalidOperationException("托盘图标资源缺失。");
        using var sourceIcon = new Icon(iconStream);

        _notifyIcon = new NotifyIcon
        {
            Icon = (Icon)sourceIcon.Clone(),
            Text = Truncate(window.Title, 63),
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.MouseClick += OnMouseClick;
        logger.LogInformation("System tray icon started");
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_exiting || _notifyIcon is null || _window is null)
            return;

        e.Cancel = true;
        _window.Hide();
        _window.ShowInTaskbar = false;
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _window is null)
            return;

        if (_window.IsVisible && _window.WindowState != WindowState.Minimized)
        {
            _window.Hide();
            _window.ShowInTaskbar = false;
        }
        else
        {
            ShowWindow();
        }
    }

    private void ShowWindow()
    {
        if (_window is null)
            return;

        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.ShowInTaskbar = true;
        _window.Activate();
    }

    private void Exit()
    {
        if (!dialogs.Confirm("即将退出宇正群邮，是否继续？", "温馨提醒"))
            return;

        _exiting = true;
        Dispose();
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_window is not null)
            _window.Closing -= OnWindowClosing;

        if (_notifyIcon is not null)
        {
            _notifyIcon.MouseClick -= OnMouseClick;
            _notifyIcon.Visible = false;
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.Icon?.Dispose();
            _notifyIcon.Dispose();
        }

        _notifyIcon = null;
        _window = null;
    }

    private static string Truncate(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..maximumLength];
}
