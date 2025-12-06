using System.Collections.Generic;
using System.Linq;
using UZonMail.Utils.Plugin;

namespace Uzon.Utils.Plugin
{
    /// <summary>
    /// 插件程序集加载上下文管理器
    /// </summary>
    public static class PluginLoadContextsManager
    {
        private static readonly Dictionary<string, PluginAssemblyLoadContext> _pluginContexts = [];

        #region 上下文管理
        /// <summary>
        /// Returns a list containing all currently loaded plugin assembly load contexts.
        /// </summary>
        /// <returns>A list of <see cref="PluginAssemblyLoadContext"/> instances representing all loaded plugin contexts. The
        /// list will be empty if no plugin contexts are loaded.</returns>
        public static List<PluginAssemblyLoadContext> All()
        {
            return [.. _pluginContexts.Select(p => p.Value)];
        }

        /// <summary>
        /// Determines whether a plugin context with the specified name exists.
        /// </summary>
        /// <param name="alcName">The name of the plugin context to locate. Cannot be null.</param>
        /// <returns>true if a plugin context with the specified name exists; otherwise, false.</returns>
        public static bool Any(string alcName)
        {
            return _pluginContexts.ContainsKey(alcName);
        }

        /// <summary>
        /// Unloads and removes the plugin context associated with the specified plugin name.
        /// </summary>
        /// <remarks>If the specified plugin name does not exist, the method performs no action. This
        /// method is static and affects the global plugin context collection.</remarks>
        /// <param name="alcName">The name of the plugin whose context should be unloaded and removed. Cannot be null.</param>
        public static void Unload(string alcName)
        {
            if (_pluginContexts.ContainsKey(alcName))
            {
                _pluginContexts[alcName].Unload();
                _pluginContexts.Remove(alcName);
            }
        }

        /// <summary>
        /// Retrieves the plugin assembly load context associated with the specified name.
        /// </summary>
        /// <param name="alcName">The unique name of the plugin assembly load context to retrieve. Cannot be null.</param>
        /// <returns>The <see cref="PluginAssemblyLoadContext"/> instance associated with the specified name.</returns>
        public static PluginAssemblyLoadContext? Get(string alcName)
        {
            if (_pluginContexts.TryGetValue(alcName, out var alc))
                return alc;
            return null;
        }

        /// <summary>
        /// Adds a plugin assembly load context to the collection using the specified name as the key.
        /// </summary>
        /// <param name="alcName">The unique name to associate with the plugin assembly load context. Cannot be null or empty.</param>
        /// <param name="context">The plugin assembly load context to add. Cannot be null.</param>
        public static void Add(string alcName, PluginAssemblyLoadContext context)
        {
            _pluginContexts.TryAdd(alcName, context);
        }
        #endregion
    }
}
