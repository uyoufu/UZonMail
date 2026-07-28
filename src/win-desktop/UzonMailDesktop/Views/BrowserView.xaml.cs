using Microsoft.Web.WebView2.Core;
using UzonMailDesktop.ViewModels;

namespace UzonMailDesktop.Views;

public partial class BrowserView : System.Windows.Controls.UserControl
{
    public BrowserView()
    {
        InitializeComponent();
    }

    private void OnInitializationCompleted(
        object? sender,
        CoreWebView2InitializationCompletedEventArgs e
    )
    {
        if (!e.IsSuccess && DataContext is BrowserViewModel viewModel)
            viewModel.ErrorMessage = $"WebView2 初始化失败：{e.InitializationException?.Message}";
        else if (
            e.IsSuccess
            && sender is Microsoft.Web.WebView2.Wpf.WebView2 webView
            && webView.CoreWebView2 is { } coreWebView
            && DataContext is BrowserViewModel initializedViewModel
        )
        {
            try
            {
                initializedViewModel.RegisterHostObjects(coreWebView);
            }
            catch (Exception exception)
            {
                initializedViewModel.ErrorMessage = $"桌面端通信初始化失败：{exception.Message}";
            }
        }
    }
}
