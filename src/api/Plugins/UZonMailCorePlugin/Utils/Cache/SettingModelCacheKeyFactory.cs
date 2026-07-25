using System.Reflection;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Utils.Cache;

/// <summary>
/// 根据设置模型元数据生成应用设置缓存键。
/// </summary>
public static class SettingModelCacheKeyFactory
{
    /// <summary>
    /// 获取设置模型映射到数据库的设置名称。
    /// </summary>
    public static string GetSettingKey<T>()
    {
        var settingKeyAttribute = typeof(T).GetCustomAttribute<EntityCacheKeyAttribute>();
        return string.IsNullOrEmpty(settingKeyAttribute?.Key)
            ? typeof(T).Name
            : settingKeyAttribute.Key;
    }

    /// <summary>
    /// 获取指定层级和所有者的设置缓存键。
    /// </summary>
    public static AppSettingCacheKey Create<T>(AppSettingType settingType, long ownerId) =>
        new(settingType, ownerId, GetSettingKey<T>());
}
