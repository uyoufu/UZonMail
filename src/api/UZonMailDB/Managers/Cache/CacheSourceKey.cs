namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 标识一个可被派生缓存依赖的强类型原始数据源。
/// </summary>
/// <typeparam name="TValue">原始数据快照类型。</typeparam>
/// <typeparam name="TIdentity">数据源业务标识类型。</typeparam>
public readonly record struct CacheSourceKey<TValue, TIdentity>(TIdentity Identity)
    where TValue : notnull
    where TIdentity : notnull
{
    internal CacheStorageKey ToStorageKey() => new(typeof(TValue), typeof(TIdentity), Identity);
}

internal readonly record struct CacheStorageKey(Type ValueType, Type IdentityType, object Identity);
