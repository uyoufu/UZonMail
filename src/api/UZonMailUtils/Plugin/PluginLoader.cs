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

namespace UzonMail.Utils.Plugin
{
    /// <summary>
    /// 按程序集依赖顺序加载插件，并负责插件服务和应用程序配置。
    /// </summary>
    public sealed class PluginLoader : IPlugin, IDisposable
    {
        private const string SharedPluginAssemblyDirectoryName = "Assembly";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PluginLoader));
        private readonly object _syncRoot = new();
        private readonly PluginAssemblyCatalog _catalog;
        private readonly List<Assembly> _pluginAssemblies = [];
        private readonly HashSet<string> _registeredPluginAssemblies =
            new(StringComparer.OrdinalIgnoreCase);
        private IReadOnlyList<LoadedPlugin> _plugins = [];
        private bool _isDisposed;

        /// <summary>
        /// 初始化插件加载器并完成插件程序集发现和实例化。
        /// </summary>
        /// <param name="pluginDirectory">插件根目录。</param>
        public PluginLoader(string pluginDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
            var absolutePluginDirectory = Path.GetFullPath(pluginDirectory);
            if (!Directory.Exists(absolutePluginDirectory))
            {
                _catalog = PluginAssemblyCatalog.CreateEmpty();
                _logger.Info($"插件目录不存在，跳过插件加载: {absolutePluginDirectory}");
                return;
            }

            var sharedAssemblyDirectory = Directory.GetParent(absolutePluginDirectory)?.FullName;
            _catalog = PluginAssemblyCatalog.Create(
                absolutePluginDirectory,
                sharedAssemblyDirectory is null
                    ? null
                    : System.IO.Path.Combine(
                        sharedAssemblyDirectory,
                        SharedPluginAssemblyDirectoryName
                    )
            );
            foreach (var duplicatePlugin in _catalog.DuplicatePlugins)
            {
                _logger.Info(
                    $"插件 {duplicatePlugin.AssemblyName} 存在相同副本，使用 {duplicatePlugin.CanonicalPath}，跳过: {string.Join(", ", duplicatePlugin.DuplicatePaths)}"
                );
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            try
            {
                foreach (var pluginMetadata in PluginDependencySorter.Sort(_catalog.Plugins))
                {
                    LoadPluginAssembly(pluginMetadata);
                }

                _plugins = PluginExecutionSorter.Sort(_plugins);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 插件加载器自身的优先级；实际插件顺序由依赖和各插件优先级共同决定。
        /// </summary>
        public int Priority => 0;

        /// <summary>
        /// 按依赖和优先级顺序配置所有插件服务。
        /// </summary>
        /// <param name="hostBuilder">宿主构建器。</param>
        public void ConfigureServices(IHostApplicationBuilder hostBuilder)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            foreach (var plugin in _plugins)
            {
                plugin.Instance.ConfigureServices(hostBuilder);
            }
        }

        /// <summary>
        /// 登记插件 MVC 程序集，并按依赖和优先级顺序配置应用程序。
        /// </summary>
        /// <param name="app">应用程序构建器。</param>
        public void ConfigureApp(IApplicationBuilder app)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            var applicationPartManager =
                app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            foreach (var assembly in _pluginAssemblies)
            {
                var isRegistered = applicationPartManager
                    .ApplicationParts.OfType<AssemblyPart>()
                    .Any(part => part.Assembly == assembly);
                if (!isRegistered)
                {
                    applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            }

            foreach (var plugin in _plugins)
            {
                plugin.Instance.ConfigureApp(app);
            }
        }

        /// <summary>
        /// 解除全局程序集解析事件订阅。
        /// </summary>
        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_isDisposed)
                    return;

                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
                _isDisposed = true;
            }
        }

        private Assembly LoadPluginAssembly(ManagedAssemblyMetadata pluginMetadata)
        {
            var assembly = FindLoadedAssembly(pluginMetadata.Identity);
            if (assembly is null)
            {
                var conflictingAssembly = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .Where(candidate => !candidate.IsDynamic)
                    .FirstOrDefault(candidate =>
                        string.Equals(
                            candidate.GetName().Name,
                            pluginMetadata.Name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
                if (conflictingAssembly is not null)
                {
                    throw new InvalidOperationException(
                        $"插件 {pluginMetadata.Name} 与已加载程序集版本冲突。请求 {pluginMetadata.Identity.FullName}，已加载 {conflictingAssembly.FullName}。"
                    );
                }

                assembly = Assembly.LoadFrom(pluginMetadata.Path);
            }

            RegisterPluginAssembly(pluginMetadata, assembly);
            return assembly;
        }

        private void RegisterPluginAssembly(
            ManagedAssemblyMetadata pluginMetadata,
            Assembly assembly
        )
        {
            lock (_syncRoot)
            {
                var assemblyIdentity = assembly.FullName ?? pluginMetadata.Identity.FullName!;
                if (_registeredPluginAssemblies.Contains(assemblyIdentity))
                    return;

                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    var loaderErrors = exception
                        .LoaderExceptions.OfType<Exception>()
                        .Select(error => error.Message);
                    throw new InvalidOperationException(
                        $"读取插件 {pluginMetadata.Name} 类型失败，路径: {pluginMetadata.Path}。{string.Join(" | ", loaderErrors)}",
                        exception
                    );
                }

                var pluginTypes = assemblyTypes
                    .Where(type =>
                        type is { IsAbstract: false, IsInterface: false }
                        && typeof(IPlugin).IsAssignableFrom(type)
                        && type != typeof(PluginLoader)
                    )
                    .OrderBy(type => type.FullName, StringComparer.Ordinal)
                    .ToList();
                if (pluginTypes.Count == 0)
                {
                    _logger.Warn($"程序集名称符合插件约定但未找到 IPlugin 实现: {pluginMetadata.Path}");
                    _registeredPluginAssemblies.Add(assemblyIdentity);
                    return;
                }

                var loadedPlugins = new List<LoadedPlugin>(pluginTypes.Count);
                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(pluginType) is not IPlugin plugin)
                        {
                            throw new InvalidOperationException(
                                $"类型 {pluginType.FullName} 无法创建为 IPlugin。"
                            );
                        }

                        loadedPlugins.Add(new LoadedPlugin(pluginMetadata, plugin));
                    }
                    catch (Exception exception) when (exception is not InvalidOperationException)
                    {
                        throw new InvalidOperationException(
                            $"实例化插件 {pluginType.FullName} 失败，程序集路径: {pluginMetadata.Path}",
                            exception
                        );
                    }
                }

                _registeredPluginAssemblies.Add(assemblyIdentity);
                _pluginAssemblies.Add(assembly);
                _plugins = [.. _plugins, .. loadedPlugins];
                _logger.Info($"已加载插件程序集: {pluginMetadata.Name}，路径: {pluginMetadata.Path}");
            }
        }

        private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            if (_isDisposed)
                return null;

            var requestedIdentity = new AssemblyName(args.Name);
            var loadedAssembly = FindLoadedAssembly(requestedIdentity);
            if (loadedAssembly is not null)
                return loadedAssembly;

            var requestedName = requestedIdentity.Name;
            if (string.IsNullOrWhiteSpace(requestedName))
                return null;

            var candidates = _catalog.GetAssemblyCandidates(requestedName);
            if (candidates.Count == 0)
                return null;

            var exactCandidates = candidates
                .Where(candidate =>
                    string.Equals(
                        candidate.Identity.FullName,
                        requestedIdentity.FullName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();
            var compatibleCandidates = exactCandidates.Count > 0 ? exactCandidates : candidates;
            var candidateIdentities = compatibleCandidates
                .Select(candidate => candidate.Identity.FullName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (candidateIdentities.Count > 1)
            {
                throw new FileLoadException(
                    $"依赖 {requestedIdentity.FullName} 在插件目录中存在多个不兼容版本: {string.Join(", ", candidateIdentities)}"
                );
            }

            var requestingDirectory = GetAssemblyDirectory(args.RequestingAssembly);
            var selectedCandidate = compatibleCandidates
                .OrderByDescending(candidate =>
                    string.Equals(
                        System.IO.Path.GetDirectoryName(candidate.Path),
                        requestingDirectory,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
                .First();

            try
            {
                var assembly =
                    FindLoadedAssembly(selectedCandidate.Identity)
                    ?? Assembly.LoadFrom(selectedCandidate.Path);
                if (_catalog.TryGetPlugin(selectedCandidate.Name, out var pluginMetadata))
                {
                    RegisterPluginAssembly(pluginMetadata, assembly);
                }

                _logger.Debug($"加载插件依赖: {requestedIdentity.FullName}，路径: {selectedCandidate.Path}");
                return assembly;
            }
            catch (Exception exception) when (exception is not FileLoadException)
            {
                throw new FileLoadException(
                    $"加载插件依赖 {requestedIdentity.FullName} 失败，路径: {selectedCandidate.Path}",
                    exception
                );
            }
        }

        private static Assembly? FindLoadedAssembly(AssemblyName identity)
        {
            return AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .FirstOrDefault(assembly =>
                    string.Equals(
                        assembly.FullName,
                        identity.FullName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }

        private static string? GetAssemblyDirectory(Assembly? assembly)
        {
            if (
                assembly is null
                || assembly.IsDynamic
                || string.IsNullOrWhiteSpace(assembly.Location)
            )
                return null;

            return System.IO.Path.GetDirectoryName(assembly.Location);
        }
    }
}
