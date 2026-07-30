using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using UzonMailDesktop.Localization;
using UzonMailDesktop.WebMessage.HostObjects;

namespace UzonMailDesktop.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    private readonly IHostObjectRegistry _hostObjectRegistry;
    private readonly IDesktopLocalizationService _localization;
    private DesktopTextKey? _errorTextKey;
    private object?[] _errorFormatArguments = [];

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Uri Url { get; }

    public BrowserViewModel(
        Uri url,
        IHostObjectRegistry hostObjectRegistry,
        IDesktopLocalizationService localization
    )
    {
        Url = url;
        _hostObjectRegistry = hostObjectRegistry;
        _localization = localization;
        _localization.LocaleChanged += OnLocaleChanged;
    }

    /// <summary>
    /// 在 WebView 初始化完成后注册桌面端能力
    /// </summary>
    public void RegisterHostObjects(CoreWebView2 webView) => _hostObjectRegistry.Register(webView);

    /// <summary>
    /// 记录 WebView 初始化失败原因并以当前语言显示
    /// </summary>
    public void SetWebViewInitializationError(Exception exception) =>
        SetError(DesktopTextKey.WebViewInitializationFailed, exception.Message);

    /// <summary>
    /// 记录桌面端宿主对象注册失败原因并以当前语言显示
    /// </summary>
    public void SetHostObjectInitializationError(Exception exception) =>
        SetError(DesktopTextKey.HostObjectInitializationFailed, exception.Message);

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    private void SetError(DesktopTextKey key, params object?[] formatArguments)
    {
        _errorTextKey = key;
        _errorFormatArguments = formatArguments;
        ErrorMessage = _localization.GetText(key, formatArguments);
    }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        if (_errorTextKey is not { } errorTextKey)
            return;

        System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => ErrorMessage = _localization.GetText(errorTextKey, _errorFormatArguments)
        );
    }
}
