using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 多级设置
    /// 子级设置优先
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <param name="ownerId"></param>
    public abstract class JsonSettings : JObject
    {
        /// <summary>
        /// 对应的设置 Id
        /// </summary>
        public long AppSettingSqlId { get; private set; }

        // 状态标记
        // 2, 忽略，跟随父级; 0, 禁用; 1, 启用
        // 默认为 2
        public AppSettingStatus Status { get; private set; } = AppSettingStatus.Enabled;

        // 版本号
        public int Version { get; private set; }

        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool IsDirty { get; private set; } = true;

        public void SetDirty()
        {
            // 不重复更新
            if (IsDirty)
                return;

            IsDirty = true;
            Version++;
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        public virtual async Task Update(SqlContext sqlContext)
        {
            if (!IsDirty)
                return;
            IsDirty = false;

            // 获取用户设置
            var userSetting = await FetchAppSetting(sqlContext);
            // 未找到设置时，跳过当前设置
            if (userSetting == null)
            {
                // 将当前设置置空
                AppSettingSqlId = 0;
                Status = AppSettingStatus.Ignored;

                RemoveAll();
            }
            // 更新设置到当前对象
            else if (userSetting.Json is JObject json)
            {
                // 合并设置，数组使用替换模式
                Merge(
                    json,
                    new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace }
                );
            }

            // 保存 Id
            if (userSetting != null)
                AppSettingSqlId = userSetting.Id;

            // 更新 status
            Status = this.SelectTokenOrDefault(
                nameof(Status).TrimStart('_'),
                AppSettingStatus.Ignored
            );
        }

        /// <summary>
        /// 从数据库拉取 AppSetting 对象
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        protected abstract Task<AppSetting?> FetchAppSetting(SqlContext sqlContext);
    }
}
