using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;
using UzonMailDesktop.Services;

namespace UzonMailDesktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? currentPage;

    public string Title { get; } = $"宇正群邮 - {Assembly.GetExecutingAssembly().GetName().Version}";

    public MainWindowViewModel(INavigationService navigation, StartupViewModel startup)
    {
        CurrentPage = startup;
        navigation.Navigated += page => CurrentPage = page;
    }
}
