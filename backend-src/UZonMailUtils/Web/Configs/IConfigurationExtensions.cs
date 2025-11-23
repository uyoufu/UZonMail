using System;
using Microsoft.Extensions.Configuration;

namespace UZonMail.Utils.Web.Configs
{
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// 通过配置获取配置项
        /// 要求配置项的名称和配置项的类型名称一致，若类型有 Config 后缀，配置中不应有 Config 后缀
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static TConfig GetConfig<TConfig>(this IConfiguration configuration)
            where TConfig : class
        {
            // 实例化配置项
            var config = Activator.CreateInstance<TConfig>();
            // 获取配置名称
            var configName = AppSettingsHelper.GetConfigFieldName<TConfig>();
            // 绑定配置
            configuration.GetSection(configName).Bind(config);
            return config;
        }
    }
}
