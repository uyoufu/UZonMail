using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.CorePlugin.Utils.Cache;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Settings;

/// <summary>
/// 提供继承设置模型的依赖驱动缓存访问。
/// </summary>
public sealed class AppSettingsManager(IDBCacheManager cacheManager) : ISingletonService
{
    /// <summary>
    /// 获取指定所有者的设置；依赖变化时自动返回重新构建的新实例。
    /// </summary>
    public Task<T> GetSetting<T>(
        SqlContext sqlContext,
        long ownerId,
        AppSettingType appSettingType = AppSettingType.User,
        CancellationToken cancellationToken = default
    )
        where T : BaseSettingModel, new()
    {
        var settingKey = SettingModelCacheKeyFactory.Create<T>(appSettingType, ownerId);
        return cacheManager.GetCache<T, SqlContext, AppSettingCacheKey>(
            sqlContext,
            settingKey,
            cancellationToken
        );
    }

    /// <summary>
    /// 在数据库提交成功后发布最新原始设置快照。
    /// </summary>
    public Task SetAppSettingSourceAsync(
        AppSetting setting,
        CancellationToken cancellationToken = default
    )
    {
        var settingKey = AppSettingCacheKey.FromSetting(setting);
        return cacheManager.SetSourceAsync(
            AppSettingSnapshot.GetSourceKey(settingKey),
            AppSettingSnapshot.FromSetting(setting),
            cancellationToken
        );
    }
}
