
using StackExchange.Redis;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.Cache
{
    public class RedisCacheAdapter : ICacheAdapter
    {
        private ConnectionMultiplexer _redis;
        private IDatabaseAsync _db;

        public RedisCacheAdapter(RedisConnectionConfig redisConfig)
        {
            var _redis = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
            _redis.ConnectionFailed += (sender, args) =>
            {
                // 链接失败
                throw new Exception($"Redis 连接失败: {args.FailureType}", args.Exception);
            };
            _redis.ConnectionRestored += (sender, args) =>
            {
                // 链接成功
                _db = _redis.GetDatabase(redisConfig.Database);
            };
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

        public async Task RemoveByPrefix(string prefix)
        {
            var redisServer = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = redisServer.Keys(pattern: prefix + "*");
            foreach (var key in keys)
            {
                await _db.KeyDeleteAsync(key);
            }
        }

        public async Task<bool> SetAsync<T>(string key, T? value)
        {
            // 将数据转为 json
            return await _db.SetAddAsync(key, value.ToJson());
        }
    }
}
