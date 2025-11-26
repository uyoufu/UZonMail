using System.Linq;
using System.Text.RegularExpressions;

namespace UZonMail.Utils.Web.Configs
{
    public partial class AppSettingsHelper
    {
        /// <summary>
        /// 获取配置字段
        /// 格式为：TConfig => Config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>配置字段名，如 a:b:c </returns>
        public static string GetConfigFieldName<T>()
            where T : class
        {
            // 判断是否有 OptionNameAttribute 特性
            // 若有，则使用特性中的名称, 该特性不从继承中获取

            string? typeName;
            if (
                typeof(T).GetCustomAttributes(typeof(OptionNameAttribute), false).FirstOrDefault()
                    is OptionNameAttribute optionNameAttr
                && !string.IsNullOrEmpty(optionNameAttr.OptionName)
            )
            {
                typeName = optionNameAttr.OptionName;
                // 名称可能是以 . 分隔的路径形式，转换成配置字段形式
                typeName = typeName.Replace('.', ':');
            }
            else
            {
                var regex = TrimEndConfigRegex();
                typeName = regex.Replace(typeof(T).Name, "");
            }
            return typeName!;
        }

        [GeneratedRegex("Config$", RegexOptions.IgnoreCase)]
        private static partial Regex TrimEndConfigRegex();
    }
}
