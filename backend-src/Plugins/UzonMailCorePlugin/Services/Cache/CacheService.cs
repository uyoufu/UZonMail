using UZonMail.Utils.Database.Redis;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.Cache
{
    /// <summary>
    /// 获取缓存服务
    /// 若 redis 不可用，则使用内存作为缓存
    /// 该服务通过 UseCacheExtension 注入
    /// </summary>
    public class CacheService : ISingletonService, ICacheAdapter
    {
        private ICacheAdapter _cacheAdapter;
        public bool IsRedis { get; private set; }

        /// <summary>
        /// 缓存服务单例
        /// </summary>
        public CacheService(IConfiguration configuration)
        {
            // 初始化 Redis
            var redisConfig = new RedisConnectionConfig();
            configuration.GetSection("Database:Redis").Bind(redisConfig);
            if (redisConfig.Enable)
            {
                _cacheAdapter = new RedisCacheAdapter(redisConfig);
                IsRedis = true;
                return;
            }

            // 若 redis 不可用，则使用内存作为缓存
            _cacheAdapter = new MemoryCacheAdapter();
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpirat">过期日期</param>
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(
            string key,
            T? value,
            TimeSpan? absoluteExpirationRelativeToNow = null
        )
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return false;

            return await _cacheAdapter.SetAsync(key, value, absoluteExpirationRelativeToNow);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default;

            return await _cacheAdapter.GetAsync<T>(key);
        }

        /// <summary>
        /// 是否存在缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> KeyExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return await _cacheAdapter.KeyExistsAsync(key);
        }

        /// <summary>
        /// 通过前缀删除缓存
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async Task RemoveByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return;

            await _cacheAdapter.RemoveByPrefix(prefix);
        }

        /// <summary>
        /// 按照 key 删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> KeyDeleteAsync(string key)
        {
            return await _cacheAdapter.KeyDeleteAsync(key);
        }
    }
}
