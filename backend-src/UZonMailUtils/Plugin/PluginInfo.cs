using System.Reflection;

namespace UZonMail.Utils.Plugin
{
    public class PluginInfo(string pluginDllPath, IPlugin pluginEntry, Assembly? pluginAssembly)
    {
        public string PluginDllPath { get; private set; } = pluginDllPath;

        public IPlugin PluginEntry { get; private set; } = pluginEntry;

        /// <summary>
        /// Assembly for plugin loader to load controllers
        /// </summary>
        public Assembly? AssemblyForController { get; private set; } = pluginAssembly;
    }
}
