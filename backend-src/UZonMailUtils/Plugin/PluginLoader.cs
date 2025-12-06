using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UZonMail.Utils.Plugin
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

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; }

        public PluginLoader(string pluginDir)
        {
            _pluginDir = pluginDir;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            LoadPlugins();
        }

        private List<string>? _allDllNames;

        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            // 第一次调用时，获取所有的 dll 名称
            _allDllNames ??= [.. Directory.GetFiles("./", "*.dll", SearchOption.AllDirectories)];

            var dllName = args.Name.Split(',').First() + ".dll";

            // 插件相互引用时，要到各自的目录中去加载
            // TODO: 目前由插件自己控制，需保证不引用其它插件依赖
            var dllFullName = _allDllNames.Where(x => x.EndsWith(dllName)).FirstOrDefault();

            if (dllFullName == null)
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
            _pluginDllFullPaths =
            [
                .. Directory
                    .GetFiles(_pluginDir, "*Plugin.dll", SearchOption.AllDirectories)
                    .Distinct()
            ];

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
            if (existPlugin != null)
                return existPlugin;

            // 判断是否存在
            var existAssembly = AppDomain
                .CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => Path.GetFileName(x.Location) == dllName);
            if (existAssembly != null)
                return existAssembly;

            var dll = Assembly.LoadFrom(assemblyPath);
            // 判断是否是插件命名约定，若不是，直接返回
            if (!assemblyPath.EndsWith("Plugin.dll"))
                return dll;

            var thisType = typeof(PluginLoader);
            var pluginTypes = dll.GetTypes()
                .Where(x => !x.IsAbstract && typeof(IPlugin).IsAssignableFrom(x) && x != thisType)
                .ToList();
            if (pluginTypes.Count > 0)
                _pluginAssemblies.Add(dll);

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

        public void ConfigureApp(IApplicationBuilder webApplication)
        {
            var provider = webApplication.ApplicationServices;
            var applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            foreach (var assembly in _pluginAssemblies)
            {
                applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));
            }

            // 配置插件
            foreach (var item in _plugins)
            {
                item.ConfigureApp(webApplication);
            }
        }

        public void ConfigureServices(IHostApplicationBuilder webApplicationBuilder)
        {
            foreach (var item in _plugins)
            {
                item.ConfigureServices(webApplicationBuilder);
            }
        }
    }
}
