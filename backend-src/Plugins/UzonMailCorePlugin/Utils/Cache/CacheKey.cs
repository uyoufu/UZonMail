using System.Reflection;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.CorePlugin.Utils.Cache
{
    /// <summary>
    /// 可以将该对象当成缓存的复合 key 来使用
    /// </summary>
    public class CacheKey(AppSettingType settingType, long ownerId, string key)
        : IAppSettingCacheArg
    {
        public AppSettingType SettingType => settingType;

        /// <summary>
        /// 拥有者的 id
        /// 系统的 id 为 0
        /// </summary>
        public long OwnerId => ownerId;

        /// <summary>
        /// 具体缓存的 key
        /// </summary>
        public string Key => key;

        public static string GetEntityKey<T>()
        {
            var key = typeof(T).Name;
            // 判断是否存在 SettingKeyAttribute
            var settingKeyAttr = typeof(T).GetCustomAttribute<EntityCacheKeyAttribute>();
            if (settingKeyAttr == null)
                return key;
            return string.IsNullOrEmpty(settingKeyAttr.Key) ? key : settingKeyAttr.Key;
        }

        /// <summary>
        /// 获取缓存 key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="systemId">系统 id</param>
        /// <param name="organizationId">组织 id，为 0 时表示非组织级级缓存</param>
        /// <param name="userId">用户id，可为 0，为 0 时表示非用户级缓存</param>
        /// <returns></returns>
        public static CacheKey GetCacheKey<T>(AppSettingType cacheLevel, long ownerId)
        {
            var entityKey = GetEntityKey<T>();
            return new CacheKey(cacheLevel, ownerId, entityKey);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not CacheKey other)
            {
                return false;
            }
            return SettingType == other.SettingType && OwnerId == other.OwnerId && Key == other.Key;
        }

        public override string ToString()
        {
            return $"{Key}:{SettingType}:{ownerId}";
        }
    }
}
