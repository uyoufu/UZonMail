using System.Threading.Tasks;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.CorePlugin.Utils.Cache;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.MySql;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.Settings
{
    /// <summary>
    /// 设置管理中心
    /// 为 Singleton 是为了方便在 Singleton 服务中使用
    /// </summary>
    public class AppSettingsManager : ISingletonService
    {
        /// <summary>
        /// 获取设置
        /// 若设置未变动，返回原来的设置
        /// 若设置修改，则返回新的设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetSetting<T>(
            SqlContext sqlContext,
            long ownerId,
            AppSettingType appSettingType = AppSettingType.User
        )
            where T : BaseSettingModel, new()
        {
            var cacheKey = CacheKey.GetCacheKey<T>(appSettingType, ownerId);

            var _allSettings = SettingModelsCache.Instance.AllSettingModels;
            // 使用 Lazy<Task<BaseSettingModel>> 确保并发时只有一个初始化任务被执行
            var lazy = _allSettings.GetOrAdd(
                cacheKey,
                _ => new Lazy<Task<BaseSettingModel>>(
                    () => CreateAndInitAsync<T>(cacheKey, sqlContext)
                )
            );
            try
            {
                if (cacheKey == null)
                {
                    ;
                }
                var result = (T)await lazy.Value;
                return result;
            }
            catch
            {
                // 如果初始化失败，移除缓存项以便后续重试（避免保留 faulted task）
                _allSettings.TryRemove(cacheKey, out _);
                throw;
            }
        }

        /// <summary>
        /// 创建并初始化设置模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        private static async Task<BaseSettingModel> CreateAndInitAsync<T>(
            CacheKey cacheKey,
            SqlContext sqlContext
        )
            where T : BaseSettingModel, new()
        {
            var setting = new T();
            await setting.UpdateModel(cacheKey, sqlContext);
            return setting;
        }

        /// <summary>
        /// 重置设置
        /// 该方法非线程安全
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingId">设置对应的数据库id</param>
        public async Task ResetSetting<T>(AppSetting setting, SqlContext db)
            where T : BaseSettingModel, new()
        {
            // 将设置缓存标记为脏
            var cacheKey = CacheKey.GetCacheKey<T>(
                setting.Type,
                setting.Type == AppSettingType.Organization
                    ? setting.OrganizationId
                    : setting.UserId
            );

            // 在被获取时，会被重新加载
            DBCacheManager.Global.SetCacheDirty<AppSettingCache, CacheKey>(cacheKey);

            // 更新 T 类型的设置的缓存
            var _allSettings = SettingModelsCache.Instance.AllSettingModels;
            // 更新设置模板
            var lazy = new Lazy<Task<BaseSettingModel>>(() => CreateAndInitAsync<T>(cacheKey, db));
            _allSettings[cacheKey] = lazy;

            await lazy.Value;
        }
    }
}
