using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using UzonMail.Utils.Plugin;

namespace UzonMailDotNET.Test.UzonMail.Utils.Plugin;

/// <summary>
/// 验证插件依赖和配置优先级排序。
/// </summary>
[TestClass]
public sealed class PluginDependencySorterTests
{
    [TestMethod]
    public void Sort_LoadsDependencyBeforeDependentPlugin()
    {
        var corePlugin = CreateMetadata("CorePlugin");
        var proPlugin = CreateMetadata("ProPlugin", "CorePlugin");

        var sortedPlugins = PluginDependencySorter.Sort([proPlugin, corePlugin]);

        Assert.AreEqual("CorePlugin", sortedPlugins[0].Name);
        Assert.AreEqual("ProPlugin", sortedPlugins[1].Name);
    }

    [TestMethod]
    public void Sort_ReportsCompleteCircularDependency()
    {
        var pluginA = CreateMetadata("PluginA", "PluginB");
        var pluginB = CreateMetadata("PluginB", "PluginC");
        var pluginC = CreateMetadata("PluginC", "PluginA");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => PluginDependencySorter.Sort([pluginA, pluginB, pluginC])
        );

        StringAssert.Contains(exception.Message, "PluginA");
        StringAssert.Contains(exception.Message, "PluginB");
        StringAssert.Contains(exception.Message, "PluginC");
    }

    [TestMethod]
    public void ExecutionSort_DependencyTakesPrecedenceOverPriority()
    {
        var dependency = CreateMetadata("DependencyPlugin");
        var dependent = CreateMetadata("DependentPlugin", "DependencyPlugin");
        var sortedPlugins = PluginExecutionSorter.Sort(
            [
                new LoadedPlugin(dependent, new TestPlugin(-100)),
                new LoadedPlugin(dependency, new TestPlugin(100)),
            ]
        );

        Assert.AreEqual("DependencyPlugin", sortedPlugins[0].Assembly.Name);
        Assert.AreEqual("DependentPlugin", sortedPlugins[1].Assembly.Name);
    }

    [TestMethod]
    public void ExecutionSort_UsesPriorityForIndependentPlugins()
    {
        var lowPriority = CreateMetadata("LowPriorityPlugin");
        var highPriority = CreateMetadata("HighPriorityPlugin");
        var sortedPlugins = PluginExecutionSorter.Sort(
            [
                new LoadedPlugin(lowPriority, new TestPlugin(10)),
                new LoadedPlugin(highPriority, new TestPlugin(-10)),
            ]
        );

        Assert.AreEqual("HighPriorityPlugin", sortedPlugins[0].Assembly.Name);
        Assert.AreEqual("LowPriorityPlugin", sortedPlugins[1].Assembly.Name);
    }

    private static ManagedAssemblyMetadata CreateMetadata(
        string assemblyName,
        params string[] references
    )
    {
        return new ManagedAssemblyMetadata(
            $"{assemblyName}.dll",
            new AssemblyName($"{assemblyName}, Version=1.0.0.0"),
            Guid.NewGuid(),
            references.ToHashSet(StringComparer.OrdinalIgnoreCase)
        );
    }

    private sealed class TestPlugin(int priority) : IPlugin
    {
        int IPlugin.Priority => priority;

        void IPlugin.ConfigureServices(IHostApplicationBuilder hostBuilder) { }

        void IPlugin.ConfigureApp(IApplicationBuilder app) { }
    }
}
