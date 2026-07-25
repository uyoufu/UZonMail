namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 缓存结果构建上下文，负责读取原始快照并自动登记依赖版本。
/// </summary>
public sealed class CacheBuildContext
{
    private readonly DBCacheManager _cacheManager;
    private readonly Dictionary<CacheStorageKey, long> _dependencies = [];

    internal CacheBuildContext(DBCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    internal IReadOnlyDictionary<CacheStorageKey, long> Dependencies => _dependencies;

    /// <summary>
    /// 获取原始数据快照；缓存不存在或已失效时仅执行一次加载委托。
    /// </summary>
    public async Task<TValue> GetSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        Func<CancellationToken, Task<TValue>> sourceLoader,
        CancellationToken cancellationToken = default
    )
        where TValue : notnull
        where TIdentity : notnull
    {
        var sourceRead = await _cacheManager.GetSourceAsync(
            sourceKey,
            sourceLoader,
            cancellationToken
        );
        _dependencies[sourceKey.ToStorageKey()] = sourceRead.Version;
        return sourceRead.Value;
    }
}
