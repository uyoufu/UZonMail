using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.Core.Utils.Cache;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Settings
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
            if (!_allSettings.TryGetValue(cacheKey, out var setting))
            {
                // 不存在时，创建设置模型
                setting = new T();
                _allSettings.TryAdd(cacheKey, setting);
            }
            // 判断是否需要更新
            await setting.UpdateModel(cacheKey, sqlContext);

            return (T)setting;
        }

        /// <summary>
        /// 重置设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingId">设置对应的数据库id</param>
        public void ResetSetting<T>(AppSetting setting)
        {
            DBCacheManager.Global.SetCacheDirty<AppSettingCache, CacheKey>(
                new CacheKey(
                    setting.Type,
                    Math.Max(setting.UserId, setting.OrganizationId),
                    setting.Key
                )
            );
        }
    }
}
