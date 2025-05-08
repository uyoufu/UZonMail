using Innofactor.EfCoreJsonValueConverter;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Settings
{
    public enum AppSettingType
    {
        System = 1,
        Organization = 2,
        User = 4
    }

    /// <summary>
    /// 系统级设置全系统只此一份
    /// 每一条记录是一个设置项
    /// </summary>
    public class AppSetting : SqlId
    {
        /// <summary>
        /// 用户 Id
        /// </summary>
        public long UserId { get; set; } = 0;

        /// <summary>
        /// 组织 Id
        /// </summary>
        public long OrganizationId { get; set; } = 0;

        /// <summary>
        /// 系统设置类型
        /// 为空是为了兼容旧数据
        /// </summary>
        public AppSettingType? Type { get; set; } = AppSettingType.System;

        /// <summary>
        /// 名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 字符串值
        /// </summary>
        public string? StringValue { get; set; }

        /// <summary>
        /// bool 值
        /// </summary>
        public bool BoolValue { get; set; }

        /// <summary>
        /// int 类型值
        /// </summary>
        public int IntValue { get; set; }

        /// <summary>
        /// Long 类型的值
        /// </summary>
        public long LongValue { get; set; }

        /// <summary>
        /// 日期时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Json 数据
        /// </summary>
        [JsonField]
        public JToken? Json { get; set; }

        #region 静态字符串
        public static string BaseApiUrl => "baseApiUrl";
        #endregion
    }
}
