using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace UzonMailDesktop.Services;

internal sealed class DialogService : IDialogService
{
    public bool Confirm(string message, string title) =>
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes
        ) == MessageBoxResult.Yes;

    public void ShowError(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
