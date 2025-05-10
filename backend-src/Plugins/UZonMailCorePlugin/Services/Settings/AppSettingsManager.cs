using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.Core.Services.Settings.Core;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;
using System.Threading.Tasks;

namespace UZonMail.Core.Services.Settings
{
    /// <summary>
    /// 设置服务
    /// TODO: 后期可以考虑将设置进行缓存
    /// </summary>
    public class AppSettingsManager : ISingletonService
    {
        private readonly ConcurrentDictionary<string, HierarchicalSetting> _allSettings = [];
        private readonly ConcurrentDictionary<string, BaseSettingModel> _settingModels = [];

        /// <summary>
        /// 获取设置
        /// 若设置未变动，返回原来的设置
        /// 若设置修改，则返回新的设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetSetting<T>(SqlContext sqlContext, long ownerId, AppSettingType appSettingType = AppSettingType.User) where T : BaseSettingModel, new()
        {
            var settingKey = GetCacheKey<T>(ownerId, appSettingType);

            if (!_allSettings.TryGetValue(settingKey, out var setting))
            {
                setting = await CreateHierarchicalSetting<T>(sqlContext, appSettingType, ownerId);
            }

            // 判断是否需要更新
            await setting.Update(sqlContext);

            var cacheKey = GetCacheKey<T>(ownerId, appSettingType);
            if (_settingModels.TryGetValue(cacheKey, out var settingModel))
            {
                // 存在，不需要更新时，直接返回
                if (settingModel.SettingHash == setting.GetHash()) return (T)settingModel;

                // 移除原来的设置
                _settingModels.TryRemove(cacheKey, out _);
            }

            // 转换成对应的设置
            var instance = new T();
            instance.SetHierarchicalSetting(setting);
            _settingModels.TryAdd(cacheKey, instance);

            return instance;
        }

        public static string GetSettingEntityKey<T>()
        {
            var key = typeof(T).Name;
            // 判断是否存在 SettingKeyAttribute
            var settingKeyAttr = typeof(T).GetCustomAttribute<SettingEntityKeyAttribute>();
            if (settingKeyAttr == null) return key;
            return string.IsNullOrEmpty(settingKeyAttr.Key) ? key : settingKeyAttr.Key;
        }

        private static string GetCacheKey<T>(long ownerId, AppSettingType appSettingType)
        {
            var settingName = GetSettingEntityKey<T>();
            return $"{settingName}:{appSettingType}:{ownerId}";
        }

        /// <summary>
        /// 创建层级设置
        /// 里面会自动添加到缓存中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlContext"></param>
        /// <param name="appSettingType"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<HierarchicalSetting> CreateHierarchicalSetting<T>(SqlContext sqlContext, AppSettingType appSettingType, long ownerId)
        {
            HierarchicalSetting result;
            var settingKey = GetSettingEntityKey<T>();

            if (appSettingType == AppSettingType.User)
            {
                // 用户配置
                var userCacheKey = GetCacheKey<T>(ownerId, AppSettingType.User);
                if (_allSettings.TryGetValue(userCacheKey, out var userSetting))
                {
                    result = userSetting;
                    return result;
                }

                // 新建配置
                result = new UserSetting(settingKey, ownerId);
                _allSettings.TryAdd(userCacheKey, result);

                var user = await sqlContext.Users
                    .Where(x => x.Id == ownerId)
                    .FirstAsync();

                // 获取组织设置
                var parent = await CreateHierarchicalSetting<OrganizationSetting>(sqlContext, AppSettingType.Organization, user.OrganizationId);
                result.Parent = parent;

                return result;
            }

            if (appSettingType == AppSettingType.Organization)
            {
                var orgCacheKey = GetCacheKey<T>(ownerId, AppSettingType.Organization);
                if (_allSettings.TryGetValue(orgCacheKey, out var orgSetting))
                {
                    result = orgSetting;
                    return result;
                }

                // 新建配置
                result = new OrganizationSetting(settingKey, ownerId);
                _allSettings.TryAdd(orgCacheKey, result);

                // 获取系统设置
                var parent = await CreateHierarchicalSetting<SystemSetting>(sqlContext, AppSettingType.System, 0);
                result.Parent = parent;
                return result;
            }

            if (appSettingType == AppSettingType.System)
            {
                // 系统的 ownerId 始终为 0
                var systemCacheKey = GetCacheKey<T>(0, AppSettingType.System);
                if (_allSettings.TryGetValue(systemCacheKey, out var systemSetting))
                {
                    result = systemSetting;
                    return result;
                }

                // 新建配置
                result = new SystemSetting(settingKey);
                _allSettings.TryAdd(systemCacheKey, result);
                return result;
            }

            throw new ArgumentException($"不支持的设置类型: {appSettingType}");
        }

        /// <summary>
        /// 重置设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingId">设置对应的数据库id</param>
        public async Task ResetSetting<T>(long appSettingId)
        {
            var settings = _allSettings.Values.Where(x => x.AppSettingId == appSettingId).ToList();
            settings.ForEach(x => x.SetDirty());

            // 如果未找到任何设置，则说明是新的设置
            // 需要更新设置同类设置
            if (settings.Count > 0) return;

            var entityKey = GetSettingEntityKey<T>();
            var updatingKeys = _allSettings.Keys.Where(x => x.StartsWith(entityKey));
            foreach(var key in updatingKeys)
            {
                if(!_allSettings.TryGetValue(key, out var setting))
                {
                    continue;
                }

                // 开始更新
                setting.SetEmptySettingDirty();
            }
        }
    }
}
