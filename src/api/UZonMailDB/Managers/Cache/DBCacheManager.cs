using System.Collections.Concurrent;
using UzonMail.DB.SQL;

namespace UzonMail.DB.Managers.Cache
{
    /// <summary>
    /// 数据库缓存管理模块
    /// 该模块的优点：
    /// 1. 延迟数据加载
    /// 2. 自动更新数据
    /// </summary>
    public class DBCacheManager
    {
        private static class CacheContainer<TKey, TResult>
            where TResult : IDBCache, new()
            where TKey : notnull
        {
            // 这里的 Value 直接就是 TResult，不再是 IDBCache 接口
            public static readonly ConcurrentDictionary<TKey, TResult> Dictionary = [];
        }

        public static readonly Lazy<DBCacheManager> _instance = new(() => new DBCacheManager());

        /// <summary>
        /// 全局缓存管理器
        /// 需要自己处理更新
        /// </summary>
        public static DBCacheManager Global => _instance.Value;

        /// <summary>
        /// 获取完整的 Key
        /// 完整的 key = 类型名:key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public DBCacheKey GetDbCacheKey<T, TArg>(TArg arg)
            where T : IDBCache, new()
        {
            return new DBCacheKey(typeof(T), arg?.ToString() ?? "default");
        }

        public async Task<TResult> GetCache<TResult, TSqlContext, TArg>(TSqlContext db, TArg arg)
            where TSqlContext : SqlContextBase
            where TResult : BaseDBCache<TSqlContext, TArg>, new()
        {
            var cacheKey = GetDbCacheKey<TResult, TArg>(arg);
            var value = CacheContainer<DBCacheKey, TResult>.Dictionary.GetOrAdd(
                cacheKey,
                (key) =>
                {
                    var newValue = new TResult();
                    newValue.SetParams(arg);
                    return newValue;
                }
            );

            // 调用进行更新
            // 若不存在，或者数据为 dirty 时，才会触发。
            await value.TryUpdate(db);
            return value;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sqlId">建议使用id</param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult, TSqlContext>(TSqlContext db, long sqlId)
            where TSqlContext : SqlContextBase
            where TResult : BaseDBCache<TSqlContext, long>, new()
        {
            return await GetCache<TResult, TSqlContext, long>(db, sqlId);
        }

        #region SqlContext 重载

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="db"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult>(SqlContext db, long arg)
            where TResult : BaseDBCache<SqlContext, long>, new()
        {
            return await GetCache<TResult, SqlContext>(db, arg);
        }

        /// <summary>
        /// 获取设置缓存
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="db"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public async Task<TResult> GetCache<TResult>(SqlContext db, IAppSettingCacheArg arg)
            where TResult : BaseDBCache<SqlContext, IAppSettingCacheArg>, new()
        {
            return await GetCache<TResult, SqlContext, IAppSettingCacheArg>(db, arg);
        }
        #endregion

        #region 标记更新
        /// <summary>
        /// 标记需要更新
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlId"></param>
        /// <returns></returns>
        public bool SetCacheDirty<TResult>(long sqlId)
            where TResult : IDBCache, new()
        {
            return SetCacheDirty<TResult, long>(sqlId);
        }

        /// <summary>
        /// 标记需要更新
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool SetCacheDirty<TResult, TArg>(TArg arg)
            where TResult : IDBCache, new()
        {
            var cacheKey = GetDbCacheKey<TResult, TArg>(arg);
            if (
                !CacheContainer<DBCacheKey, TResult>.Dictionary.TryGetValue(cacheKey, out var value)
            )
                return false;

            value.SetDirty();
            return true;
        }
        #endregion
    }
}
