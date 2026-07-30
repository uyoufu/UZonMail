using UzonMailDesktop.ViewModels;

namespace UzonMailDesktop.Views;

public partial class BrowserView : System.Windows.Controls.UserControl
{
    private bool _hasNavigated;

    public BrowserView()
    {
        InitializeComponent();
    }

    private async void OnBrowserLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_hasNavigated || DataContext is not BrowserViewModel viewModel)
            return;

        try
        {
            await Browser.EnsureCoreWebView2Async();
        }
        catch (Exception exception)
        {
            viewModel.SetWebViewInitializationError(exception);
            return;
        }

        try
        {
            viewModel.RegisterHostObjects(Browser.CoreWebView2);
            _hasNavigated = true;
            Browser.CoreWebView2.Navigate(viewModel.Url.AbsoluteUri);
        }
        catch (Exception exception)
        {
            viewModel.SetHostObjectInitializationError(exception);
        }
    }
}
