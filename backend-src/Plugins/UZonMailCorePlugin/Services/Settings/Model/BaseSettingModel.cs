using Newtonsoft.Json;
using UZonMail.Core.Utils.Cache;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.Settings.Model
{
    /// <summary>
    /// 所有设置的基类
    /// 获取设置时，状态已经在更底层处理，此类只需要对值进行初始化后即可使用
    /// </summary>
    public abstract class BaseSettingModel
    {
        [JsonIgnore]
        private readonly List<CacheKey> _settingKeyChains = [];

        [JsonIgnore]
        private List<AppSettingCache> _appSettings = [];

        /// <summary>
        /// 系统设置状态
        /// 默认为启用
        /// </summary>
        public AppSettingStatus Status { get; set; } = AppSettingStatus.Enabled;

        /// <summary>
        /// 获取 json 设置
        /// 若设置有变动，则会重新获取
        /// </summary>
        /// <returns></returns>
        public async Task UpdateModel(CacheKey cacheKey, SqlContext db)
        {
            // 根据 key 生成设置链
            await GenerateSettingKeyChain(cacheKey, db);

            // 通过设置链获取所有设置
            var settings = await GetAppSettings(db);
            if (settings.Count == 0)
            {
                return;
            }

            // 通过所有设置生成 hash 值
            var newHash = ComputeSettingsHash(settings);
            if (newHash == _currentHash)
            {
                return;
            }
            _currentHash = newHash;
            _appSettings = settings;

            // 若 hash 值有变动，则重新读取并更新设置值
            ReadValuesFromJsons();
        }

        private async Task GenerateSettingKeyChain(CacheKey cacheKey, SqlContext db)
        {
            _settingKeyChains.Clear();

            // 添加自己
            _settingKeyChains.Add(cacheKey);

            if (cacheKey.SettingType == AppSettingType.User)
            {
                // 获取用户信息
                var userInfo = await DBCacheManager.Global.GetCache<UserInfoCache>(
                    db,
                    cacheKey.OwnerId
                );

                // 添加组织设置
                _settingKeyChains.Add(
                    new CacheKey(AppSettingType.Organization, userInfo.OrganizationId, cacheKey.Key)
                );
            }

            // 添加系统设置
            if (cacheKey.SettingType != AppSettingType.System)
            {
                _settingKeyChains.Add(new CacheKey(AppSettingType.System, 0, cacheKey.Key));
            }
        }

        private async Task<List<AppSettingCache>> GetAppSettings(SqlContext db)
        {
            var results = new List<AppSettingCache>();
            foreach (var cacheKey in _settingKeyChains)
            {
                var setting = await DBCacheManager.Global.GetCache<AppSettingCache>(db, cacheKey);
                if (setting != null)
                {
                    results.Add(setting);
                }
            }
            return results;
        }

        private static string ComputeSettingsHash(List<AppSettingCache> settings)
        {
            var hashes = settings.Select(x => $"{x.Id}:{x.Version}");
            return string.Join("_", hashes);
        }

        private string _currentHash = string.Empty;

        /// <summary>
        /// 从 Json 初始化值
        /// 因为设置对象是动态生成的
        /// </summary>
        protected abstract void ReadValuesFromJsons();

        /// <summary>
        /// 获取所有设置值，包括父级设置
        /// 目前只适配了基本类型的值
        /// 顺序：子->父
        /// 单独的值通过 match 进行匹配
        /// 一组值通过 status 进行匹配，若 status 为 Disabled，则不再向上查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">key会自动转换成 cameCase</param>
        /// <returns></returns>
        public (bool, T) GetValue<T>(string key, Func<T, bool> match)
        {
            key = key.ToCamelCase();
            for (var index = 0; index < _appSettings.Count; index++)
            {
                var jsonSetting = _appSettings[index];
                var status = jsonSetting.JsonData.SelectTokenOrDefault(
                    nameof(Status).ToCamelCase(),
                    // 默认启用，兼容旧版本
                    AppSettingStatus.Enabled
                );

                // 忽略时，跳过
                if (status == AppSettingStatus.Ignored)
                    continue;

                // 启用时，获取值
                if (status == AppSettingStatus.Enabled)
                {
                    // 获取值
                    var value = jsonSetting.JsonData.SelectTokenOrDefault<T>(key, default);
                    // 值非空，且满足要求，返回该值
                    if (value != null && match(value))
                    {
                        return (true, value);
                    }
                }

                // 禁用时，不再向上查找
                if (status == AppSettingStatus.Disabled)
                {
                    break;
                }
            }

            return (false, default);
        }

        /// <summary>
        /// 获取设置的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetStringValue(string key, string defaultValue = "")
        {
            var (ok, value) = GetValue<string>(key, x => !string.IsNullOrEmpty(x));
            return ok ? value : defaultValue;
        }

        /// <summary>
        /// 获取 double 类型的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double GetDoubleValue(string key, double defaultValue = -1)
        {
            var (ok, value) = GetValue<double>(key, x => x >= 0);
            return ok ? value : defaultValue;
        }

        /// <summary>
        /// 获取第一个非负数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetIntValue(string key, int defaultValue = -1)
        {
            var (ok, value) = GetValue<int>(key, x => x >= 0);
            return ok ? value : defaultValue;
        }

        /// <summary>
        /// 获取第一个非负数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public long GetLongValue(string key, long defaultValue = -1)
        {
            var (ok, value) = GetValue<long>(key, x => x >= 0);
            return ok ? value : defaultValue;
        }

        /// <summary>
        /// 获取 bool 类型的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            var (ok, value) = GetValue<bool>(key, x => true);
            return ok ? value : defaultValue;
        }
    }
}
