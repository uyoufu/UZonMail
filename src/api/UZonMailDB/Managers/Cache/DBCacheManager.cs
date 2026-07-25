using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UzonMail.DB.SQL;
using UzonMail.Utils.Web.Service;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 维护原始数据快照和依赖这些快照的派生缓存结果。
/// </summary>
public sealed class DBCacheManager
    : IDBCacheManager,
        ISingletonService<IDBCacheManager>,
        IDisposable
{
    private sealed record SourceCacheEntry(object? Value, long Version, bool IsValid);

    private sealed record ResultCacheEntry(
        object Value,
        IReadOnlyDictionary<CacheStorageKey, long> Dependencies
    );

    private readonly MemoryCache _sourceCache;
    private readonly MemoryCache _resultCache;
    private readonly MemoryCacheEntryOptions _sourceEntryOptions;
    private readonly MemoryCacheEntryOptions _resultEntryOptions;
    private readonly KeyedAsyncLock<CacheStorageKey> _sourceLocks = new();
    private readonly KeyedAsyncLock<CacheStorageKey> _resultLocks = new();
    private long _nextSourceVersion;

    /// <summary>
    /// 使用应用配置创建进程内数据库缓存管理器。
    /// </summary>
    public DBCacheManager(IOptions<DBCacheOptions> options)
    {
        var cacheOptions = options.Value;
        cacheOptions.Validate();

        var memoryCacheOptions = new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMinutes(
                cacheOptions.ExpirationScanFrequencyMinutes
            )
        };
        _sourceCache = new MemoryCache(memoryCacheOptions);
        _resultCache = new MemoryCache(memoryCacheOptions);

        var slidingExpiration = TimeSpan.FromMinutes(cacheOptions.SlidingExpirationMinutes);
        _sourceEntryOptions = new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration };
        _resultEntryOptions = new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration };
    }

    /// <inheritdoc />
    public async Task<TResult> GetCache<TResult, TSqlContext, TArg>(
        TSqlContext db,
        TArg arg,
        CancellationToken cancellationToken = default
    )
        where TSqlContext : SqlContextBase
        where TResult : BaseDBCache<TSqlContext, TArg>, new()
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(arg);

        var resultKey = new CacheStorageKey(typeof(TResult), typeof(TArg), arg);
        if (TryGetFreshResult<TResult>(resultKey, out var cachedResult))
            return cachedResult;

        using (await _resultLocks.AcquireAsync(resultKey, cancellationToken))
        {
            if (TryGetFreshResult<TResult>(resultKey, out cachedResult))
                return cachedResult;

            // 数据源可能在构建过程中变化；只有版本快照稳定的结果才能发布。
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var buildContext = new CacheBuildContext(this);
                var newResult = new TResult();
                await newResult.InitializeAsync(buildContext, db, arg, cancellationToken);

                if (buildContext.Dependencies.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"缓存结果 {typeof(TResult).FullName} 未登记任何原始数据依赖。"
                    );
                }

                if (!AreDependenciesCurrent(buildContext.Dependencies))
                    continue;

                var dependencies = new Dictionary<CacheStorageKey, long>(buildContext.Dependencies);
                _resultCache.Set(
                    resultKey,
                    new ResultCacheEntry(newResult, dependencies),
                    _resultEntryOptions
                );
                return newResult;
            }
        }
    }

    /// <inheritdoc />
    public Task<TResult> GetCache<TResult, TSqlContext>(
        TSqlContext db,
        long sqlId,
        CancellationToken cancellationToken = default
    )
        where TSqlContext : SqlContextBase
        where TResult : BaseDBCache<TSqlContext, long>, new() =>
        GetCache<TResult, TSqlContext, long>(db, sqlId, cancellationToken);

    /// <inheritdoc />
    public Task<TResult> GetCache<TResult>(
        SqlContext db,
        long sqlId,
        CancellationToken cancellationToken = default
    )
        where TResult : BaseDBCache<SqlContext, long>, new() =>
        GetCache<TResult, SqlContext, long>(db, sqlId, cancellationToken);

    /// <inheritdoc />
    public async Task SetSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        TValue value,
        CancellationToken cancellationToken = default
    )
        where TValue : notnull
        where TIdentity : notnull
    {
        ArgumentNullException.ThrowIfNull(value);
        var storageKey = sourceKey.ToStorageKey();
        using (await _sourceLocks.AcquireAsync(storageKey, cancellationToken))
        {
            SetSourceEntry(storageKey, value, isValid: true);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        CancellationToken cancellationToken = default
    )
        where TValue : notnull
        where TIdentity : notnull
    {
        var storageKey = sourceKey.ToStorageKey();
        using (await _sourceLocks.AcquireAsync(storageKey, cancellationToken))
        {
            SetSourceEntry(storageKey, value: null, isValid: false);
        }
    }

    internal async Task<SourceRead<TValue>> GetSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        Func<CancellationToken, Task<TValue>> sourceLoader,
        CancellationToken cancellationToken
    )
        where TValue : notnull
        where TIdentity : notnull
    {
        ArgumentNullException.ThrowIfNull(sourceLoader);
        var storageKey = sourceKey.ToStorageKey();
        if (TryGetSource(storageKey, out SourceRead<TValue> sourceRead))
            return sourceRead;

        using (await _sourceLocks.AcquireAsync(storageKey, cancellationToken))
        {
            if (TryGetSource(storageKey, out sourceRead))
                return sourceRead;

            var loadedValue = await sourceLoader(cancellationToken);
            ArgumentNullException.ThrowIfNull(loadedValue);
            var sourceEntry = SetSourceEntry(storageKey, loadedValue, isValid: true);
            return new SourceRead<TValue>(loadedValue, sourceEntry.Version);
        }
    }

    private bool TryGetFreshResult<TResult>(CacheStorageKey resultKey, out TResult result)
    {
        if (
            _resultCache.TryGetValue(resultKey, out ResultCacheEntry? resultEntry)
            && resultEntry is not null
            && resultEntry.Value is TResult typedResult
            && AreDependenciesCurrent(resultEntry.Dependencies)
        )
        {
            result = typedResult;
            return true;
        }

        result = default!;
        return false;
    }

    private bool AreDependenciesCurrent(IReadOnlyDictionary<CacheStorageKey, long> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            if (
                !_sourceCache.TryGetValue(dependency.Key, out SourceCacheEntry? currentSource)
                || currentSource is not { IsValid: true }
                || currentSource.Version != dependency.Value
            )
            {
                return false;
            }
        }

        return true;
    }

    private bool TryGetSource<TValue>(CacheStorageKey storageKey, out SourceRead<TValue> sourceRead)
        where TValue : notnull
    {
        if (
            _sourceCache.TryGetValue(storageKey, out SourceCacheEntry? sourceEntry)
            && sourceEntry is { IsValid: true, Value: TValue value }
        )
        {
            sourceRead = new SourceRead<TValue>(value, sourceEntry.Version);
            return true;
        }

        sourceRead = default;
        return false;
    }

    private SourceCacheEntry SetSourceEntry(CacheStorageKey storageKey, object? value, bool isValid)
    {
        var version = Interlocked.Increment(ref _nextSourceVersion);
        var sourceEntry = new SourceCacheEntry(value, version, isValid);
        _sourceCache.Set(storageKey, sourceEntry, _sourceEntryOptions);
        return sourceEntry;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _sourceCache.Dispose();
        _resultCache.Dispose();
    }
}

internal readonly record struct SourceRead<TValue>(TValue Value, long Version)
    where TValue : notnull;
