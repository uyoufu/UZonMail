using Innofactor.EfCoreJsonValueConverter;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Settings
{
    /// <summary>
    /// 系统级设置全系统只此一份
    /// 每一条记录是一个设置项
    /// </summary>
    public class SystemSetting : SqlId
    {
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
