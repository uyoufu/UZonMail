using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 树形设置
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <param name="ownerId"></param>
    public abstract class HierarchicalSetting(AppSettingType type, string key, long ownerId) : JObject
    {
        /// <summary>
        /// 设置类型
        /// </summary>
        public AppSettingType SettingType => type;

        public string Key => key;

        /// <summary>
        /// Id
        /// </summary>
        public long OwnerId => ownerId;

        /// <summary>
        /// 对应的设置 Id
        /// </summary>
        public long AppSettingId { get; private set; }

        private int _version = 0;
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool Dirty { get; private set; } = true;

        public void SetDirty()
        {
            // 不重复更新
            if (Dirty) return;

            Dirty = true;
            _version++;
        }

        /// <summary>
        /// 对空设置的项标记为需要更新
        /// 该操作会向上递归
        /// </summary>
        public void SetEmptySettingDirty()
        {
            if (AppSettingId > 0) return;

            SetDirty();
            Parent?.SetEmptySettingDirty();
        }

        /// <summary>
        /// 获取当前对象的 Hash 值
        /// </summary>
        /// <returns></returns>
        public int GetHash()
        {
            var prentHash = Parent?.GetHash() ?? 0;
            return HashCode.Combine(prentHash, _version);
        }

        /// <summary>
        /// 父级设置
        /// </summary>
        [JsonIgnore]
        public HierarchicalSetting Parent { get; set; }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        public virtual async Task Update(SqlContext sqlContext)
        {
            if (!Dirty) return;
            Dirty = false;

            // 获取用户设置
            var userSetting = await FetchAppSetting(sqlContext);
            if (userSetting == null)
            {
                // 将当前设置置空
                AppSettingId = 0;
                RemoveAll();
            }
            // 更新设置到当前对象
            else if (userSetting.Json is JObject json)
            {
                foreach (var property in json.Properties())
                {
                    if (property.Value is JToken token)
                    {
                        this[property.Name] = token;
                    }
                }
            }

            // 保存 Id
            if (userSetting != null) AppSettingId = userSetting.Id;

            // 更新父级
            if (Parent != null) await Parent.Update(sqlContext);
        }

        /// <summary>
        /// 从数据库拉取 AppSetting 对象
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        protected abstract Task<AppSetting?> FetchAppSetting(SqlContext sqlContext);

        /// <summary>
        /// 获取所有设置值，包括父级设置
        /// 目前只适配了基本类型的值
        /// 顺序：子->父
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T?> GetValues<T>(string key)
        {
            List<T?> results = [];
            var value = this[key];
            if (value != null)
            {
                var tValue = value.ToObject<T>();
                // 值非空，添加到结果中
                if (tValue != null)
                {
                    results.Add(tValue);
                }
            }

            if (Parent != null)
            {
                // 获取父级设置
                var parentValue = Parent.GetValues<T>(key);
                results.AddRange(parentValue);
            }
            return results;
        }

        /// <summary>
        /// 将当前对象转换为指定类型的列表
        /// 顺序：子->父
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> ToObjects<T>()
        {
            var results = new List<T>();
            var value = this.ToObject<T>();
            if (value != null) results.Add(value);

            var parentValue = Parent?.ToObjects<T>();
            if(parentValue!=null)results.AddRange(parentValue);

            return results;
        }
    }
}
