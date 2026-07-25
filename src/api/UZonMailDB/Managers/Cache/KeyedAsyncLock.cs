using System.Collections.Concurrent;

namespace UzonMail.DB.Managers.Cache;

internal sealed class KeyedAsyncLock<TKey>
    where TKey : notnull
{
    private sealed class LockState
    {
        public object SyncRoot { get; } = new();

        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int ReferenceCount { get; set; }

        public bool IsRemoved { get; set; }
    }

    private readonly ConcurrentDictionary<TKey, LockState> _lockStates = [];

    public async Task<IDisposable> AcquireAsync(
        TKey key,
        CancellationToken cancellationToken = default
    )
    {
        while (true)
        {
            var lockState = _lockStates.GetOrAdd(key, static _ => new LockState());
            lock (lockState.SyncRoot)
            {
                if (lockState.IsRemoved)
                    continue;

                lockState.ReferenceCount++;
            }

            try
            {
                await lockState.Semaphore.WaitAsync(cancellationToken);
                return new LockReleaser(this, key, lockState);
            }
            catch
            {
                ReleaseReference(key, lockState, shouldReleaseSemaphore: false);
                throw;
            }
        }
    }

    private void ReleaseReference(TKey key, LockState lockState, bool shouldReleaseSemaphore)
    {
        if (shouldReleaseSemaphore)
            lockState.Semaphore.Release();

        lock (lockState.SyncRoot)
        {
            lockState.ReferenceCount--;
            if (lockState.ReferenceCount != 0)
                return;

            // 删除标记和字典移除位于同一临界区，防止新调用者取得即将移除的旧锁。
            lockState.IsRemoved = true;
            _lockStates.TryRemove(new KeyValuePair<TKey, LockState>(key, lockState));
        }
    }

    private sealed class LockReleaser(KeyedAsyncLock<TKey> owner, TKey key, LockState lockState)
        : IDisposable
    {
        private int _isDisposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
                return;

            owner.ReleaseReference(key, lockState, shouldReleaseSemaphore: true);
        }
    }
}
