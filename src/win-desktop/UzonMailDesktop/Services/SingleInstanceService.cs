namespace UzonMailDesktop.Services;

internal sealed class SingleInstanceService : ISingleInstanceService
{
    private Mutex? _mutex;
    private bool _ownsMutex;

    public bool TryAcquire()
    {
        _mutex = new Mutex(initiallyOwned: true, "Local\\UzonMailDesktop", out var createdNew);
        _ownsMutex = createdNew;
        return createdNew;
    }

    public void Dispose()
    {
        if (_ownsMutex)
            _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _mutex = null;
        _ownsMutex = false;
    }
}
