using System.Collections.Generic;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace UZonMail.Utils.Plugin
{
    /// <summary>
    /// 插件加载器管理器
    /// </summary>
    public static class PluginLoadersExtension
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PluginLoadersExtension));
        private static Dictionary<string, PluginLoader> _pluginLoaders = [];

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pluginDir">默认为 Plugins 目录</param>
        public static IHostApplicationBuilder AddPlugins(
            this IHostApplicationBuilder builder,
            string pluginDir = "Plugins"
        )
        {
            // 判断目录是否已经添加过
            if (_pluginLoaders.TryGetValue(pluginDir, out var loader))
            {
                return builder;
            }

            loader = new PluginLoader(pluginDir);
            _pluginLoaders.Add(pluginDir, loader);
            loader.ConfigureServices(builder);

            return builder;
        }

        /// <summary>
        /// 使用插件
        /// 必须先调用 <see cref="AddPlugins(IServiceCollection, string)"/>"/>
        /// 需在 MapControllers 之前调用
        /// </summary>
        /// <param name="app"></param>
        /// <param name="pluginDir">插件的目录，应与 AddPlugins 一致</param>
        /// <returns></returns>
        public static IApplicationBuilder UsePlugins(
            this IApplicationBuilder app,
            string pluginDir = "Plugins"
        )
        {
            if (!_pluginLoaders.TryGetValue(pluginDir, out var loader))
            {
                _logger.Warn($"请在 Build() 之前先调用 {nameof(AddPlugins)} 方法");
                return app;
            }

            loader.ConfigureApp(app);
            return app;
        }
    }
}
