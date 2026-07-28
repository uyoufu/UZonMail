using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using UzonMailDesktop.WebMessage.HostObjects;

namespace UzonMailDesktop.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    private readonly IHostObjectRegistry _hostObjectRegistry;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Uri Url { get; }

    public BrowserViewModel(Uri url, IHostObjectRegistry hostObjectRegistry)
    {
        Url = url;
        _hostObjectRegistry = hostObjectRegistry;
    }

    /// <summary>
    /// 在 WebView 初始化完成后注册桌面端能力
    /// </summary>
    public void RegisterHostObjects(CoreWebView2 webView) => _hostObjectRegistry.Register(webView);

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
