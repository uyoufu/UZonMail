using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Uamazing.Utils.Plugin
{
    /// <summary>
    /// 插件加载器
    /// 必须在 AddControllers 之前初始化
    /// 需要保证每个插件只有一个 dll，否则可能出现重复加载的情况
    /// </summary>
    public class PluginLoader : IPlugin
    {
        private ILog _logger = LogManager.GetLogger(typeof(PluginLoader));
        private readonly string _pluginDir;
        private List<string> _pluginDllFullPaths;
        private List<Assembly> _pluginAssemblies = [];
        private List<IPlugin> _plugins = [];

        public PluginLoader(string pluginDir)
        {
            _pluginDir = pluginDir;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            LoadPlugins();
        }

        private List<string>? _allDllNames;
        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            _allDllNames ??= [.. Directory.GetFiles("./", "*.dll", SearchOption.AllDirectories)];

            var dllName = args.Name.Split(',').First() + ".dll";
            var dllFullName = _allDllNames.Where(x => x.EndsWith(dllName)).FirstOrDefault();

            if(dllFullName == null)
            {
                _logger.Warn($"未找到 dll: {dllName}");
                return null;
            }

            var absDllFullName = Path.GetFullPath(dllFullName);

            // 有可能插件间相互引用，在此处也要进行插件加载
            var assembly = LoadAssembly(absDllFullName);
            return assembly;
        }

        /// <summary>
        /// 开始加载插件
        /// </summary>
        private void LoadPlugins()
        {
            if (!Directory.Exists(_pluginDir))
            {
                return;
            }

            // 获取所有插件的 dll 名称
            _pluginDllFullPaths = [.. Directory.GetFiles(_pluginDir, "*Plugin.dll", SearchOption.AllDirectories)];

            if (_pluginDllFullPaths.Count == 0)
            {
                return;
            }

            // 加载插件
            foreach (var dllFullPath in _pluginDllFullPaths)
            {               
                LoadAssembly(dllFullPath);
            }
        }

        /// <summary>
        /// 加载单个插件
        /// </summary>
        /// <param name="assemblyPath"></param>
        private Assembly? LoadAssembly(string assemblyPath)
        {
            var dllName = Path.GetFileName(assemblyPath);

            // 防止重复加载
            var existPlugin = _pluginAssemblies.Find(x => Path.GetFileName(x.Location) == dllName);
            if (existPlugin != null) return existPlugin;

            // 判断是否存在
            var existAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => Path.GetFileName(x.Location) == dllName);
            if (existAssembly != null) return existAssembly;

            var dll = Assembly.LoadFrom(assemblyPath);
            // 判断是否是插件命名约定，若不是，直接返回
            if (!assemblyPath.EndsWith("Plugin.dll")) return dll;

            var thisType = typeof(PluginLoader);
            var pluginTypes = dll.GetTypes().Where(x => !x.IsAbstract && typeof(IPlugin).IsAssignableFrom(x) && x != thisType).ToList();
            if (pluginTypes.Count > 0) _pluginAssemblies.Add(dll);

            var pluginName = Path.GetFileNameWithoutExtension(assemblyPath);
            foreach (var pluginType in pluginTypes)
            {               
                if (Activator.CreateInstance(pluginType) is not IPlugin plugin)
                {
                    _logger.Warn($"插件 {pluginName} 未实现 IPlugin 接口");
                    continue;
                }

                // 开始加载
                _plugins.Add(plugin);                
            }
            _logger.Info($"已加载插件: {pluginName}");

            return dll;
        }

        public void AddApplicationPart(IMvcBuilder mvcBuilder)
        {
            foreach (var assembly in _pluginAssemblies)
            {
                mvcBuilder.AddApplicationPart(assembly);
            }
        }

        public void UseApp(WebApplication webApplication)
        {
            foreach (var item in _plugins)
            {
                item.UseApp(webApplication);
            }
        }

        public void UseServices(WebApplicationBuilder webApplicationBuilder)
        {
            foreach (var item in _plugins)
            {
                item.UseServices(webApplicationBuilder);
            }
        }
    }
}
