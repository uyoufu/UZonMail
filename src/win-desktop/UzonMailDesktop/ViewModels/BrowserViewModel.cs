using CommunityToolkit.Mvvm.ComponentModel;

namespace UzonMailDesktop.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Uri Url { get; }

    public BrowserViewModel(Uri url)
    {
        Url = url;
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
