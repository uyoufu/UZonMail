using UzonMail.Utils.Web.Configs;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 数据库派生缓存的内存回收配置。
/// </summary>
[OptionName("Database:Cache")]
public sealed class DBCacheOptions : IAppOptions
{
    public int SlidingExpirationMinutes { get; set; } = 30;

    public int ExpirationScanFrequencyMinutes { get; set; } = 5;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(SlidingExpirationMinutes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(ExpirationScanFrequencyMinutes, 1);
    }
}
