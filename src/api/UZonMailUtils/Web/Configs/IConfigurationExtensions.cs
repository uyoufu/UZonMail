using System;
using Microsoft.Extensions.Configuration;

namespace UzonMail.Utils.Web.Configs
{
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// 通过配置获取配置项
        /// 要求配置项名称与类型名称一致；类型名以 Options 结尾时，配置项名称不包含该后缀。
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
            var configName = AppOptionsHelper.GetOptionKey<TConfig>();
            // 绑定配置
            configuration.GetSection(configName).Bind(config);
            return config;
        }
    }
}
