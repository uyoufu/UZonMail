using System;
using System.Threading.Tasks;

namespace UZonMail.Utils.Database.Redis
{
    public interface ICacheAdapter
    {
        Task<bool> SetAsync<T>(string key, T? value, TimeSpan? absoluteExpirationRelativeToNow);

        Task<T?> GetAsync<T>(string key);

        Task<bool> KeyExistsAsync(string key);

        Task<bool> KeyDeleteAsync(string key);

        Task RemoveByPrefix(string prefix);
    }
}
