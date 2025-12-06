using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using log4net;

namespace UZonMail.Utils.Plugin
{
    /// <summary>
    /// 插件程序集加载上下文
    /// 参考：https://learn.microsoft.com/zh-cn/dotnet/standard/assembly/unloadability#create-a-collectible-assemblyloadcontext
    /// </summary>
    public class PluginAssemblyLoadContext(string mainAssemblyToLoadPath)
        : AssemblyLoadContext(isCollectible: true)
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(PluginAssemblyLoadContext)
        );
        private readonly AssemblyDependencyResolver _resolver = new(mainAssemblyToLoadPath);

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            try
            {
                // 先从默认上下文中获取程序集，若不存在时，再从自定义上下文中获取
                Assembly? assembly = Default.LoadFromAssemblyName(assemblyName);
                return assembly;
            }
            catch (Exception e)
            {
                _logger.Debug(e.Message);
                _logger.Warn($"从默认域中加载程序集 {assemblyName.Name} 失败，尝试从当前上下文中加载...");
            }

            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                //// 这种方式可以自动加载调试信息，但是会导致 dll 被锁定
                //return LoadFromAssemblyPath(assemblyPath);

                Stream? assemblySymbols = null;
#if DEBUG
                // 加载调试文件
                var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                if (File.Exists(pdbPath))
                {
                    assemblySymbols = new FileStream(pdbPath, FileMode.Open, FileAccess.Read);
                }
#endif

                // 这种方式无法加载调试信息，但是不会导致 dll 被锁定
                using var fs = new FileStream(assemblyPath, FileMode.Open);
                // 使用此方法, 就不会导致dll被锁定
                // 锁定dll 会导致: 1. 无法通过复制粘贴替换 更新 2. 无法删除
                return LoadFromStream(fs, assemblySymbols);
            }

            // Load 方法返回 null。 这意味着所有依赖项程序集都会加载到默认上下文中，而新上下文仅包含显式加载到其中的程序集
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }
            return IntPtr.Zero;
        }
    }
}
