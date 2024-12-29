using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Plugin
{
    public class PluginService : ISingletonService
    {
        private readonly Lazy<List<string>> _installedPlugins = new Lazy<List<string>>(() =>
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
