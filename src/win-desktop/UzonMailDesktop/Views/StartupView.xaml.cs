using System.Windows;
using System.Windows.Media.Animation;

namespace UzonMailDesktop.Views;

public partial class StartupView : System.Windows.Controls.UserControl
{
    public StartupView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        (Resources["EntranceAnimation"] as Storyboard)?.Begin(this);
}
