using UzonMail.DB.SQL;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 管理原始数据快照及其派生缓存结果。
/// </summary>
public interface IDBCacheManager
{
    /// <summary>
    /// 获取派生缓存结果，并在原始依赖变化后自动重建。
    /// </summary>
    Task<TResult> GetCache<TResult, TSqlContext, TArg>(
        TSqlContext db,
        TArg arg,
        CancellationToken cancellationToken = default
    )
        where TSqlContext : SqlContextBase
        where TResult : BaseDBCache<TSqlContext, TArg>, new();

    /// <summary>
    /// 使用 long 标识获取指定数据库上下文的派生缓存。
    /// </summary>
    Task<TResult> GetCache<TResult, TSqlContext>(
        TSqlContext db,
        long sqlId,
        CancellationToken cancellationToken = default
    )
        where TSqlContext : SqlContextBase
        where TResult : BaseDBCache<TSqlContext, long>, new();

    /// <summary>
    /// 使用主数据库上下文和 long 标识获取派生缓存。
    /// </summary>
    Task<TResult> GetCache<TResult>(
        SqlContext db,
        long sqlId,
        CancellationToken cancellationToken = default
    )
        where TResult : BaseDBCache<SqlContext, long>, new();

    /// <summary>
    /// 发布新的原始数据快照并提升源版本。
    /// </summary>
    Task SetSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        TValue value,
        CancellationToken cancellationToken = default
    )
        where TValue : notnull
        where TIdentity : notnull;

    /// <summary>
    /// 标记原始数据失效，使其在下次构建时重新加载。
    /// </summary>
    Task InvalidateSourceAsync<TValue, TIdentity>(
        CacheSourceKey<TValue, TIdentity> sourceKey,
        CancellationToken cancellationToken = default
    )
        where TValue : notnull
        where TIdentity : notnull;
}
