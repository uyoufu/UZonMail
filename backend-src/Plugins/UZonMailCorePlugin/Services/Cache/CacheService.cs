using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Cache
{
    /// <summary>
    /// 获取缓存服务
    /// 若 redis 不可用，则使用内存作为缓存
    /// 该服务通过 UseCacheExtension 注入
    /// </summary>
    public class CacheService : ISingletonService, ICacheAdapter
    {
        private ICacheAdapter _cacheAdapter;

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
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(string key, T? value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return false;

           return await _cacheAdapter.SetAsync(key, value);
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
    }
}
