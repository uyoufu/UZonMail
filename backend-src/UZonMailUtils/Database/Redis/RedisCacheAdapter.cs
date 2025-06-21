
using log4net;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using UZonMail.Utils.Json;

namespace UZonMail.Utils.Database.Redis
{
    public class RedisCacheAdapter : ICacheAdapter
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RedisCacheAdapter));
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabaseAsync _db;

        public bool Enable => _redis.IsConnected;

        public RedisCacheAdapter(RedisConnectionConfig redisConfig)
        {
            _redis = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
            _redis.ConnectionFailed += (sender, args) =>
            {
                // 链接失败
                _logger.Warn($"Redis 连接失败: {args.FailureType}", args.Exception);
            };
            _redis.ConnectionRestored += (sender, args) =>
            {
                // 链接成功
                _logger.Info($"Redis: {redisConfig.ConnectionString} 连接成功!");
            };
            _db = _redis.GetDatabase(redisConfig.Database);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            // 将数据转为 json
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return value.ToString().JsonTo<T>();
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        public async Task<bool> KeyDeleteAsync(string key)
        {
            // 删除指定的 key
            return await _db.KeyDeleteAsync(key);
        }

        public async Task RemoveByPrefix(string prefix)
        {
            var redisServer = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = redisServer.Keys(pattern: prefix + "*");
            foreach (var key in keys)
            {
                await _db.KeyDeleteAsync(key);
            }
        }

        public async Task<bool> SetAsync<T>(string key, T? value, TimeSpan? absoluteExpirationRelativeToNow)
        {
            // 将数据转为 json
            return await _db.StringSetAsync(key, value.ToJson(), absoluteExpirationRelativeToNow);
        }
    }
}
