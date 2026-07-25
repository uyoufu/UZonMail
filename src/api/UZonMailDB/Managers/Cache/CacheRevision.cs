namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 表示只关心版本变化、不需要承载业务数据的原始源值。
/// </summary>
public readonly record struct CacheRevision
{
    public static CacheRevision Current => new();
}
