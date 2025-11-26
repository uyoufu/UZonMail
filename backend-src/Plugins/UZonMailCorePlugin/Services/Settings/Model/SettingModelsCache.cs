using System.Collections.Concurrent;
using UZonMail.Core.Utils.Cache;

namespace UZonMail.Core.Services.Settings.Model
{
    /// <summary>
    /// 设置缓存单例
    /// </summary>
    public class SettingModelsCache
    {
        public static SettingModelsCache Instance => _instance.Value;
        private static readonly Lazy<SettingModelsCache> _instance =
            new(() => new SettingModelsCache());

        private SettingModelsCache() { }

        public readonly ConcurrentDictionary<CacheKey, BaseSettingModel> AllSettingModels = [];
    }
}
