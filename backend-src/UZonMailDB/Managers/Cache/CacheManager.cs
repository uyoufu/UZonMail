using log4net;
using UZonMail.DB.SQL;
using System.Collections.Concurrent;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 数据库缓存管理器
    /// </summary>
    public class CacheManager
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(CacheManager));
        private readonly static Lazy<CacheManager> _instance = new(() => new CacheManager());
        /// <summary>
        /// 全局缓存管理器
        /// </summary>
        public static CacheManager Global => _instance.Value;

        private readonly ConcurrentDictionary<string, IDBCache> _settingsDic = [];

        /// <summary>
        /// 获取完整的 Key
        /// 完整的 key = 类型名:key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetFullKey<T>(string key) where T : IDBCache, new()
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
        public string GetSubKey<T>(string fullKey) where T : IDBCache, new()
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
        public bool SetCacheDirty<TResult>(string objectIdKey)
            where TResult : IDBCache, new()
        {
            var fullKey = GetFullKey<TResult>(objectIdKey);
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
        public async Task<TResult> GetCache<TResult, TSqlContext>(TSqlContext db, string key)
            where TSqlContext : SqlContextBase
            where TResult : BaseDBCache<TSqlContext>, new()
        {
            var fullKey = GetFullKey<TResult>(key);

            if (!_settingsDic.TryGetValue(fullKey, out var value))
            {
                value = new TResult();
                var subKey = GetSubKey<TResult>(fullKey);
                value.SetKey(subKey);
                _settingsDic.TryAdd(fullKey, value);
            }
            await (value as TResult)!.Update(db);
            return (TResult)value;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult, TSqlContext>(TSqlContext db, long key)
            where TSqlContext : SqlContextBase
            where TResult : BaseDBCache<TSqlContext>, new()
        {
            return await GetCache<TResult, TSqlContext>(db, key.ToString());
        }

        #region SqlContext 重载
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">建议使用id</param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult>(SqlContext db, string key)
            where TResult : BaseDBCache<SqlContext>, new()
        {
            return await GetCache<TResult, SqlContext>(db, key);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult>(SqlContext db, long key)
            where TResult : BaseDBCache<SqlContext>, new()
        {
            return await GetCache<TResult, SqlContext>(db, key.ToString());
        }
        #endregion
    }
}
