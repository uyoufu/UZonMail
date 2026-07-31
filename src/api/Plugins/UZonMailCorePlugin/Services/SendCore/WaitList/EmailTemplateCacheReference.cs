namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 记录单个模板被发件组租用的状态。
/// 模板内容由 IDBCacheManager 持有，此对象只维护生命周期与同模板操作串行化。
/// </summary>
internal sealed class EmailTemplateCacheReference
{
    private readonly object _stateLock = new();
    private int _referenceCount;
    private DateTimeOffset _lastAccessUtc;

    public EmailTemplateCacheReference(DateTimeOffset createdAtUtc)
    {
        _lastAccessUtc = createdAtUtc;
    }

    /// <summary>
    /// 防止读取、显式释放和空闲回收同时操作同一个缓存结果。
    /// </summary>
    public SemaphoreSlim CacheOperationLock { get; } = new(1, 1);

    public void AddReference(DateTimeOffset accessedAtUtc)
    {
        lock (_stateLock)
        {
            _referenceCount++;
            _lastAccessUtc = accessedAtUtc;
        }
    }

    public bool ReleaseReference()
    {
        lock (_stateLock)
        {
            if (_referenceCount > 0)
                _referenceCount--;

            return _referenceCount == 0;
        }
    }

    public void MarkAccessed(DateTimeOffset accessedAtUtc)
    {
        lock (_stateLock)
        {
            _lastAccessUtc = accessedAtUtc;
        }
    }

    public bool HasNoReferences()
    {
        lock (_stateLock)
        {
            return _referenceCount == 0;
        }
    }

    public bool IsIdle(DateTimeOffset utcNow, TimeSpan idleTimeout)
    {
        lock (_stateLock)
        {
            return utcNow - _lastAccessUtc >= idleTimeout;
        }
    }
}
