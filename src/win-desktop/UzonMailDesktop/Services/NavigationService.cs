namespace UzonMailDesktop.Services;

internal sealed class NavigationService : INavigationService
{
    public event Action<object>? Navigated;

    public void Navigate(object page) => Navigated?.Invoke(page);
}
