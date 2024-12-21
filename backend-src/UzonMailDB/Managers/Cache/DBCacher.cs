using log4net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UZonMail.DB.SQL.Settings;
using UZonMail.DB.SQL;
using System.Collections.Concurrent;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 数据库缓存管理器
    /// </summary>
    public class DBCacher
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(DBCacher));
        private static readonly ConcurrentDictionary<string, ICache> _settingsDic = [];

        /// <summary>
        /// 获取完整的 Key
        /// 完整的 key = 类型名:key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetFullKey<T>(string key) where T : ICache, new()
        {
            var fullName = typeof(T).FullName;
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(fullName)) throw new ArgumentNullException("fullName");

            // 若 key 已经是完整的 key 则直接返回
            if (key.StartsWith(fullName)) return key;
            return $"{typeof(T).FullName}:{key}";
        }

        /// <summary>
        /// 获取子 key
        /// 不包含类型名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullKey"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetSubKey<T>(string fullKey) where T : ICache, new()
        {
            var fullName = typeof(T).FullName;
            if (string.IsNullOrEmpty(fullKey)) throw new ArgumentNullException(nameof(fullKey));
            if (string.IsNullOrEmpty(fullName)) throw new ArgumentNullException("fullName");

            // 若 key 已经是完整的 key 则直接返回
            if (!fullKey.StartsWith(fullName)) return fullKey;
            return fullKey[(fullName.Length + 1)..];
        }

        /// <summary>
        /// 标记 cache 需要更新
        /// </summary>
        /// <param name="objectIdKey"></param>
        /// <returns></returns>
        public static bool SetCacheDirty<T>(string objectIdKey) where T : ICache, new()
        {
            var fullKey = GetFullKey<T>(objectIdKey);
            // 移除缓存数据
            if (!_settingsDic.TryGetValue(fullKey, out var value)) return false;
            value.SetDirty(true);
            return true;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">建议使用id</param>
        /// <returns></returns>
        public static async Task<T> GetCache<T>(SqlContext db, string key) where T : ICache, new()
        {
            var fullKey = GetFullKey<T>(key);

            if (!_settingsDic.TryGetValue(fullKey, out var value))
            {
                value = new T();
                var subKey = GetSubKey<T>(fullKey);
                value.SetKey(subKey);
                _settingsDic.TryAdd(fullKey, value);
            }
            await value.Update(db);
            return (T)value;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<T> GetCache<T>(SqlContext db,long key) where T : ICache, new()
        {
            return await GetCache<T>(db, key.ToString());
        }
    }
}
