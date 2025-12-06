using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UZonMail.Utils.Database.Redis;

namespace UZonMail.CorePlugin.Services.Cache
{
    public class MemoryCacheAdapter : ICacheAdapter
    {
        private readonly MemoryCache _cache;

        public MemoryCacheAdapter()
        {
            _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public async Task<bool> KeyDeleteAsync(string key)
        {
            // 删除指定的 key
            _cache.Remove(key);
            return true;
        }

        public async Task RemoveByPrefix(string prefix)
        {
            var keyResults = _cache
                .Keys.Select(x => new { keyStr = x.ToString(), key = x })
                .Where(x => x.keyStr.StartsWith(prefix))
                .ToList();
            foreach (var keyResult in keyResults)
            {
                _cache.Remove(keyResult.key);
            }
        }

        public async Task<bool> SetAsync<T>(
            string key,
            T? value,
            TimeSpan? absoluteExpirationRelativeToNow
        )
        {
            if (absoluteExpirationRelativeToNow == null)
                _cache.Set(key, value);
            else
                _cache.Set(key, value, (TimeSpan)absoluteExpirationRelativeToNow);

            return true;
        }
    }
}
