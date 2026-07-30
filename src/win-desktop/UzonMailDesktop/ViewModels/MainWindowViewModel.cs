using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using UzonMailDesktop.Localization;
using UzonMailDesktop.Services;
using Application = System.Windows.Application;

namespace UzonMailDesktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IDesktopLocalizationService _localization;

    [ObservableProperty]
    private object? currentPage;

    public string Title =>
        _localization.GetText(
            DesktopTextKey.ApplicationTitle,
            Assembly.GetExecutingAssembly().GetName().Version
        );

    public MainWindowViewModel(
        INavigationService navigation,
        StartupViewModel startup,
        IDesktopLocalizationService localization
    )
    {
        _localization = localization;
        CurrentPage = startup;
        navigation.Navigated += page => CurrentPage = page;
        _localization.LocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(object? sender, EventArgs e) =>
        Application.Current.Dispatcher.InvokeAsync(() => OnPropertyChanged(nameof(Title)));
}
