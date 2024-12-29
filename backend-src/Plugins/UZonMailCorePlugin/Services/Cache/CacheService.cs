using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.Cache
{
    /// <summary>
    /// 获取缓存服务
    /// 若 redis 不可用，则使用内存作为缓存
    /// 该服务通过 UseCacheExtension 注入
    /// </summary>
    public class CacheService
    {
        /// <summary>
        /// 缓存服务单例
        /// </summary>
        public static CacheService Instance { get; private set; }
        private CacheService(IConfiguration configuration)
        {
            // 供外部调用
            Instance = this;

            // 初始化内存缓存
            MemoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            // 初始化 Redis
            var redisConfig = new RedisConnectionConfig();
            configuration.GetSection("Database:Redis").Bind(redisConfig);
            if (!redisConfig.Enable)
            {
                _redisEnabled = false;
                return;
            }

            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
            connectionMultiplexer.ConnectionFailed += (sender, args) =>
            {
                // 链接失败
                _redisEnabled = false;
            };
            connectionMultiplexer.ConnectionRestored += (sender, args) =>
            {
                // 链接成功
                _redisEnabled = true;
                RedisCache = connectionMultiplexer.GetDatabase(redisConfig.Database);
            };
        }

        /// <summary>
        /// 创建缓存服务
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static CacheService CreateCacheService(IConfiguration configuration)
        {
            if (Instance != null)
            {
                return Instance;
            }
            return new CacheService(configuration);
        }

        /// <summary>
        /// Redis 是否可用
        /// </summary>
        private bool _redisEnabled = false;

        /// <summary>
        /// redis 数据库
        /// </summary>
        public IDatabaseAsync RedisCache { get; private set; }

        /// <summary>
        /// 基于内存的数据库，当 redis 不可用时，使用该数据库
        /// </summary>
        public IMemoryCache MemoryCache { get; private set; }

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

            if (_redisEnabled)
            {
                // 将数据转为 json
                return await RedisCache.SetAddAsync(key, value.ToJson());
            }
            else
            {
                MemoryCache.Set(key, value);
                return true;
            }
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

            if (_redisEnabled)
            {
                // 将数据转为 json
                var value = await RedisCache.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                    return default;
                return value.ToString().JsonTo<T>();
            }
            else
            {
                return MemoryCache.Get<T>(key);
            }
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

            if (_redisEnabled)
            {
                return await RedisCache.KeyExistsAsync(key);
            }
            else
            {
                return MemoryCache.TryGetValue(key, out _);
            }
        }
    }
}
