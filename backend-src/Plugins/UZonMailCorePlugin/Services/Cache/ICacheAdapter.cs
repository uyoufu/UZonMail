namespace UZonMail.Core.Services.Cache
{
    public interface ICacheAdapter
    {
        Task<bool> SetAsync<T>(string key, T? value);

        Task<T?> GetAsync<T>(string key);

        Task<bool> KeyExistsAsync(string key);

        Task RemoveByPrefix(string prefix);
    }
}
