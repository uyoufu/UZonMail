using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 唯一标识一条系统、组织或用户设置。
/// </summary>
public readonly record struct AppSettingCacheKey(
    AppSettingType SettingType,
    long OwnerId,
    string SettingKey
)
{
    /// <summary>
    /// 从已保存的设置实体生成缓存键。
    /// </summary>
    public static AppSettingCacheKey FromSetting(AppSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);
        var ownerId = setting.Type switch
        {
            AppSettingType.Organization => setting.OrganizationId,
            AppSettingType.User => setting.UserId,
            _ => 0
        };
        return new AppSettingCacheKey(setting.Type, ownerId, setting.Key);
    }
}

/// <summary>
/// 应用设置的不可替换原始快照。
/// </summary>
public sealed record AppSettingSnapshot(long Id, JToken JsonData)
{
    /// <summary>
    /// 获取指定设置的原始数据源键。
    /// </summary>
    public static CacheSourceKey<AppSettingSnapshot, AppSettingCacheKey> GetSourceKey(
        AppSettingCacheKey settingKey
    ) => new(settingKey);

    /// <summary>
    /// 从数据库实体创建与 EF 跟踪对象隔离的快照。
    /// </summary>
    public static AppSettingSnapshot FromSetting(AppSetting setting) =>
        new(setting.Id, setting.Json?.DeepClone() ?? new JObject());

    /// <summary>
    /// 通过构建上下文读取设置，并自动登记依赖版本。
    /// </summary>
    public static Task<AppSettingSnapshot> GetAsync(
        CacheBuildContext buildContext,
        SqlContext db,
        AppSettingCacheKey settingKey,
        CancellationToken cancellationToken
    ) =>
        buildContext.GetSourceAsync(
            GetSourceKey(settingKey),
            async token =>
            {
                var query = db
                    .AppSettings.AsNoTracking()
                    .Where(x => x.Key == settingKey.SettingKey)
                    .Where(x => x.Type == settingKey.SettingType);
                query = settingKey.SettingType switch
                {
                    AppSettingType.Organization
                        => query.Where(x => x.OrganizationId == settingKey.OwnerId),
                    AppSettingType.User => query.Where(x => x.UserId == settingKey.OwnerId),
                    _ => query
                };

                var setting = await query.FirstOrDefaultAsync(token);
                return setting is null
                    ? new AppSettingSnapshot(0, new JObject())
                    : FromSetting(setting);
            },
            cancellationToken
        );
}
