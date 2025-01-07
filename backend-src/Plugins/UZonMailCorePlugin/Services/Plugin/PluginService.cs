using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Plugin
{
    /// <summary>
    /// 插件服务
    /// </summary>
    public class PluginService : ISingletonService
    {
        private readonly Lazy<List<string>> _installedPlugins = new(() =>
        {
            // 获取插件
            var allPlugins = Directory.GetFiles("./Plugins", "*Plugin.dll", SearchOption.AllDirectories);
            var pluginNames = allPlugins.Select(x => Path.GetFileNameWithoutExtension(x)).Distinct().ToList();
            return pluginNames;
        });

        /// <summary>
        /// 获取已经安装的插件名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetInstalledPluginNames()
        {
            return _installedPlugins.Value;
        }
    }
}
