using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UzonMail.Utils.Web.Configs
{
    public static partial class AppOptionsHelper
    {
        /// <summary>
        /// 获取 Options 类型对应的配置字段。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>配置字段名，如 a:b:c。</returns>
        public static string GetOptionKey<T>()
            where T : class
        {
            return GetOptionKey(typeof(T));
        }

        /// <summary>
        /// 获取 Options 类型对应的配置字段。
        /// </summary>
        /// <param name="optionType">Options 类型。</param>
        /// <returns>配置字段名，如 a:b:c。</returns>
        public static string GetOptionKey(Type optionType)
        {
            ArgumentNullException.ThrowIfNull(optionType);

            if (
                optionType.GetCustomAttributes(typeof(OptionNameAttribute), false).FirstOrDefault()
                    is OptionNameAttribute optionNameAttr
                && !string.IsNullOrEmpty(optionNameAttr.OptionName)
            )
            {
                return optionNameAttr.OptionName.Replace('.', ':');
            }

            return TrimEndConfigRegex().Replace(optionType.Name, "");
        }

        [GeneratedRegex("Options$", RegexOptions.IgnoreCase)]
        private static partial Regex TrimEndConfigRegex();
    }
}
