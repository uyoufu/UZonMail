
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace UZonMail.Core.Services.Cache
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

        public async Task RemoveByPrefix(string prefix)
        {
            var keyResults = _cache.Keys.Select(x => new { keyStr = x.ToString(), key = x })
                .Where(x => x.keyStr.StartsWith(prefix))
                .ToList();
            foreach (var keyResult in keyResults)
            {
                _cache.Remove(keyResult.key);
            }
        }

        public async Task<bool> SetAsync<T>(string key, T? value)
        {
            _cache.Set(key, value);
            return true;
        }
    }
}
