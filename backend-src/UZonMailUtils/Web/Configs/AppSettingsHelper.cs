using System.Text.RegularExpressions;

namespace UZonMail.Utils.Web.Configs
{
    public class AppSettingsHelper
    {
        /// <summary>
        /// 获取配置字段
        /// 格式为：TConfig => Config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetConfigFieldName<T>()
            where T : class
        {
            var regex = new Regex("Config$", RegexOptions.IgnoreCase);
            return regex.Replace(typeof(T).Name, "");
        }
    }
}
