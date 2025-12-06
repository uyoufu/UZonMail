using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Uzon.Utils.Plugin;

namespace UZonMail.Utils.Plugin
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    public class PluginLoader
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(PluginLoader));
        private List<PluginInfo> _pluginInfos = [];
        private IHostApplicationBuilder? _builder;

        public PluginLoader(string pluginDir)
        {
            LoadPlugins(pluginDir);
        }

        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureServices(IHostApplicationBuilder builder)
        {
            if (_builder != null)
                return;
            _builder = builder;

            ConfigureServices();
        }

        private IApplicationBuilder? _app;

        /// <summary>
        /// 设置应用程序
        /// </summary>
        /// <param name="webApplication"></param>
        public void ConfigureApp(IApplicationBuilder webApplication)
        {
            if (_app != null)
                return;
            _app = webApplication;

            ConfigureApp();
        }

        #region 插件加载与卸载
        /// <summary>
        /// 开始加载插件
        /// 只会加载 pluginDir 目录下以 Plugin.dll 结尾的文件
        /// 每个 xxxPlugin.dll 所在目录为一个 ALC(AssemblyLoadContext)
        /// </summary>
        private async Task LoadPlugins(string pluginRoot)
        {
            _logger.Info($"开始加载插件，插件目录: {pluginRoot}");

            if (!Directory.Exists(pluginRoot))
            {
                _logger.Warn($"插件目录不存在: {pluginRoot}");
                return;
            }

            // 获取所有以 Plugin.dll 结尾的文件
            // 获取所有插件的 dll 名称
            List<string> pluginDllPaths =
            [
                .. Directory.GetFiles(pluginRoot, "*Plugin.dll", SearchOption.AllDirectories)
            ];

            if (pluginDllPaths.Count == 0)
            {
                _logger.Warn($"未找到任何插件: {pluginRoot}");
                return;
            }

            // 单独的插件先完全加载
            // 插件间互相引用时，只注入 services, 不加载 controller
            // 根据插件间的依赖关系，决定加载顺序
            var mainPluginPaths = GetMainPlugins(pluginRoot, pluginDllPaths);
            var depPluginPaths = pluginDllPaths.Except(mainPluginPaths).ToList();

            LoadPluginsCore(pluginRoot, mainPluginPaths, true);
            LoadPluginsCore(pluginRoot, depPluginPaths, false);
        }

        private void LoadPluginsCore(string pluginRoot, List<string> mainPluginPaths, bool isMain)
        {
            // 加载插件
            foreach (var pluginDllPath in mainPluginPaths)
            {
                var dllName = Path.GetFileName(pluginDllPath);
                // 判断是否在当前域中已经加载
                // TODO: 由于插件的 Location 为空，此语句总是返回 false, 后期再优化
                if (
                    AppDomain
                        .CurrentDomain.GetAssemblies()
                        .Any(x =>
                        {
                            var fileName = Path.GetFileName(x.Location);
                            return fileName == dllName;
                        })
                )
                {
                    continue;
                }

                // 判断插件是否已经加载
                var alcName = GetPluginAlcName(pluginRoot, pluginDllPath);
                // 有可能插件位于 pluginDir 根目录, 若位于根目录，则 alcName 为 dllFullPath
                if (isMain && PluginLoadContextsManager.Any(alcName))
                {
                    _logger.Info($"插件 {dllName} 已加载，跳过");
                    continue;
                }

                // 如果是依赖插件，只能从主插件的 ALC 中加载
                var loadContext = PluginLoadContextsManager.Get(alcName);
                if (!isMain && loadContext == null)
                {
                    _logger.Error(alcName + " 插件的主插件未加载，无法加载该依赖插件");
                    continue;
                }

                if (isMain)
                    loadContext = new(pluginDllPath);

                // 加载插件程序集
                var assemblyName = new AssemblyName(
                    Path.GetFileNameWithoutExtension(pluginDllPath)
                );
                // 若使用 LoadFromAssemblyPath 会导致插件文件被锁定
                var pluginAssembly = loadContext!.LoadFromAssemblyName(assemblyName);

                Type[]? assemblyTypes = null;
                try
                {
                    assemblyTypes = pluginAssembly.GetTypes();
                }
                catch (Exception ex)
                {
                    _logger.Error($"加载插件 {dllName} 时发生错误: {ex.Message}");
                    continue;
                }

                var pluginTypes = pluginAssembly
                    .GetTypes()
                    .Where(x => !x.IsAbstract && typeof(IPlugin).IsAssignableFrom(x))
                    .ToList();
                if (pluginTypes.Count == 0)
                    continue;

                // 如果有多个接口实现了 IPlugin, 则只取第一个
                if (pluginTypes.Count > 1)
                {
                    _logger.Warn($"插件 {dllName} 实现了多个 IPlugin 接口, 程序只初始化一个第一个接口");
                }

                var pluginType = pluginTypes.First();
                var pluginName = Path.GetFileNameWithoutExtension(pluginDllPath);
                if (Activator.CreateInstance(pluginType) is not IPlugin plugin)
                {
                    _logger.Warn($"插件 {pluginName} 实例化时失败，请保证存在无参构造函数");
                    continue;
                }

                // 保存插件实例和程序集
                _pluginInfos.Add(
                    new PluginInfo(pluginDllPath, plugin, isMain ? pluginAssembly : null)
                );

                // 保存上下文
                PluginLoadContextsManager.Add(alcName, loadContext);

                _logger.Info($"已加载插件: {pluginName}/{pluginType.Name}");
            }
        }

        private static string GetPluginAlcName(string pluginRoot, string pluginDllPath)
        {
            var alcName = Path.GetDirectoryName(pluginDllPath)!;
            if (alcName == pluginRoot)
            {
                alcName = pluginDllPath;
            }
            return alcName;
        }

        /// <summary>
        /// 对插件进行排序，保证主插件先加载，其它插件只以服务的形式注入
        /// </summary>
        /// <param name="pluginDllPaths"></param>
        /// <returns></returns>
        private static List<string> GetMainPlugins(string pluginRoot, List<string> pluginDllPaths)
        {
            // 若插件位于根目录，则默认每个插件之间没有依赖关系，都是主插件
            var mainPlugins = pluginDllPaths
                .Where(x => Path.GetDirectoryName(x) == pluginRoot)
                .ToList();

            // 若位于子目录中，则先筛选主插件
            var pluginsQueue = new Queue<List<string>>(
                pluginDllPaths
                    .Where(x => Path.GetDirectoryName(x) != pluginRoot)
                    .GroupBy(x => Path.GetDirectoryName(x)!)
                    .Select(x => x.ToList())
            );

            // 主插件特征:
            // 1. 可能包含同名的 .deps.json 文件
            // 2. 所在目录中只有一个 xxxPlugin.dll 文件

            while (pluginsQueue.Count > 0)
            {
                var dllPaths = pluginsQueue.Dequeue();
                if (dllPaths.Count == 0)
                {
                    continue;
                }

                // 只有一个插件时，默认该插件为主插件
                if (dllPaths.Count == 1)
                {
                    mainPlugins.Add(dllPaths[0]);
                    continue;
                }

                bool foundMain = false;
                foreach (var dllPath in dllPaths)
                {
                    var depsJson = Path.ChangeExtension(dllPath, ".deps.json");
                    if (File.Exists(depsJson))
                    {
                        mainPlugins.Add(dllPath);
                        foundMain = true;
                        break;
                    }
                }

                if (!foundMain)
                {
                    // 将该组插件重新入队，等待下一轮处理
                    pluginsQueue.Enqueue([.. dllPaths.Except(mainPlugins)]);
                }
            }

            return mainPlugins;
        }
        #endregion

        /// <summary>
        /// 进行 APP 配置
        /// </summary>
        private void ConfigureApp()
        {
            if (_app == null)
            {
                return;
            }

            // 使用插件
            foreach (var item in _pluginInfos)
            {
                item.PluginEntry.ConfigureApp(_app);
            }

            // 添加动态路由
            var provider = _app.ApplicationServices;
            var applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            foreach (var assembly in _pluginInfos)
            {
                if (assembly.AssemblyForController == null)
                {
                    continue;
                }
                applicationPartManager.ApplicationParts.Add(
                    new AssemblyPart(assembly.AssemblyForController)
                );
            }
        }

        /// <summary>
        /// 添加服务
        /// 注册插件中的服务
        /// 注册插件中的配置文件
        /// 主程序的配置文件优先级高于插件中的配置文件
        /// </summary>
        private void ConfigureServices()
        {
            if (_builder == null)
            {
                return;
            }

            // 注册插件中的服务
            foreach (var pluginInfo in _pluginInfos)
            {
                // 只有 controller 才添加配置
                if (pluginInfo.AssemblyForController != null)
                    // 将插件中的配置添加到主应用程序中，若配置相同，则以主应用程序中的配置为准
                    AddConfiguration(Path.GetDirectoryName(pluginInfo.PluginDllPath)!);

                // 调用插件中的注册服务的方法
                pluginInfo.PluginEntry.ConfigureServices(_builder);
            }

            // 获取程序的根目录
            var appBaseDir = AppContext.BaseDirectory;
            AddConfiguration(appBaseDir);
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="baseDir"></param>
        private void AddConfiguration(string baseDir)
        {
            if (_builder == null)
                return;

            var path1 = Path.Combine(baseDir, "appsettings.json");
            if (File.Exists(path1))
                _builder.Configuration.AddJsonFile(path1);
            var env = _builder.Environment.EnvironmentName;
            var path2 = Path.Combine(baseDir, $"appsettings.{env}.json");
            if (File.Exists(path2))
                _builder.Configuration.AddJsonFile(path2);
        }
    }
}
