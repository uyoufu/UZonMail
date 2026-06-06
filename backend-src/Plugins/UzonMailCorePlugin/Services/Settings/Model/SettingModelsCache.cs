using System.Collections.Concurrent;
using UZonMail.CorePlugin.Utils.Cache;

namespace UZonMail.CorePlugin.Services.Settings.Model
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

        // 使用 Lazy<Task<BaseSettingModel>> 支持一次性、异步的懒初始化，避免并发创建多个实例
        public readonly ConcurrentDictionary<
            CacheKey,
            Lazy<Task<BaseSettingModel>>
        > AllSettingModels = [];
    }
}
