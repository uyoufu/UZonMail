using Newtonsoft.Json;
using UZonMail.Core.Services.Settings.Core;

namespace UZonMail.Core.Services.Settings.Model
{
    /// <summary>
    /// 所有设置的基类
    /// </summary>
    public abstract class BaseSettingModel
    {
        [JsonIgnore]
        protected HierarchicalSetting HierarchicalSetting { get; private set; }

        public int SettingHash { get; private set; }

        public void SetHierarchicalSetting(HierarchicalSetting setting)
        {
            HierarchicalSetting = setting;
            SettingHash = HierarchicalSetting.GetHash();

            InitValue();
        }

        protected abstract void InitValue();

        /// <summary>
        /// 获取设置的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetStringValue(string key, string defaultValue="")
        {
            var values = HierarchicalSetting.GetValues<string>(key);
            return values.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault() ?? defaultValue;
        }

        /// <summary>
        /// 获取 double 类型的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double GetDoubleValue(string key,double defaultValue = 0)
        {
            var values = HierarchicalSetting.GetValues<double>(key);
            if(values.Count == 0) return defaultValue;

            var positiveValues = values.Where(x => x >= 0).ToList();
            if (positiveValues.Count == 0) return defaultValue;

            return positiveValues.First();
        }

        /// <summary>
        /// 获取第一个非负数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetIntValue(string key, int defaultValue = -1)
        {
            var values = HierarchicalSetting.GetValues<int>(key);
            if (values.Count == 0) return defaultValue;

            var positiveValues = values.Where(x => x >= 0).ToList();
            if (positiveValues.Count == 0) return defaultValue;

            return positiveValues.First();
        }

        /// <summary>
        /// 获取第一个非负数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public long GetLongValue(string key, int defaultValue = -1)
        {
            var values = HierarchicalSetting.GetValues<long>(key);
            if (values.Count == 0) return defaultValue;

            var positiveValues = values.Where(x => x >= 0).ToList();
            if (positiveValues.Count == 0) return defaultValue;

            return positiveValues.First();
        }

        /// <summary>
        /// 获取 bool 类型的值 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            var values = HierarchicalSetting.GetValues<bool>(key);
            if (values.Count == 0) return defaultValue;

            return values.First();
        }
    }
}
