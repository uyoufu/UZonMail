using UzonMail.DB.SQL;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 数据库派生缓存基类。每次刷新都会构建新实例并在完成后原子发布。
/// </summary>
/// <typeparam name="TSqlContext">数据库上下文类型。</typeparam>
/// <typeparam name="TArg">派生结果业务标识类型。</typeparam>
public abstract class BaseDBCache<TSqlContext, TArg>
    where TSqlContext : SqlContextBase
{
    protected TArg Args { get; private set; } = default!;

    internal async Task InitializeAsync(
        CacheBuildContext buildContext,
        TSqlContext db,
        TArg arg,
        CancellationToken cancellationToken
    )
    {
        Args = arg;
        await UpdateCore(buildContext, db, cancellationToken);
    }

    /// <summary>
    /// 从原始数据源构建完整缓存结果。
    /// </summary>
    protected abstract Task UpdateCore(
        CacheBuildContext buildContext,
        TSqlContext db,
        CancellationToken cancellationToken
    );
}
