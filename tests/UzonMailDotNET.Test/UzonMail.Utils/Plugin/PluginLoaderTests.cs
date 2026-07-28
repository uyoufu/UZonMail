using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UzonMail.DB.SQL;
using UzonMail.ProPlugin.SQL;
using UzonMail.Utils.Plugin;
using UzonMail.Utils.Web;

namespace UzonMailDotNET.Test.UzonMail.Utils.Plugin;

/// <summary>
/// 验证真实插件程序集的发现、去重和服务配置。
/// </summary>
[TestClass]
public sealed class PluginLoaderTests
{
    [TestMethod]
    public void Catalog_DeduplicatesCopiedDependencyPluginAndSortsItFirst()
    {
        using var pluginDirectory = TemporaryPluginDirectory.Create();

        var catalog = PluginAssemblyCatalog.Create(pluginDirectory.PluginDirectoryPath);
        var sortedPlugins = PluginDependencySorter.Sort(catalog.Plugins);

        Assert.HasCount(2, sortedPlugins);
        Assert.AreEqual("UzonMail.CorePlugin", sortedPlugins[0].Name);
        Assert.AreEqual("UzonMail.ProPlugin", sortedPlugins[1].Name);
        Assert.HasCount(1, catalog.DuplicatePlugins);
        Assert.AreEqual("UzonMail.CorePlugin", catalog.DuplicatePlugins[0].AssemblyName);
    }

    [TestMethod]
    public void Catalog_IndexesSharedDependenciesWithoutDiscoveringThemAsPlugins()
    {
        using var pluginDirectory = TemporaryPluginDirectory.Create();

        var catalog = PluginAssemblyCatalog.Create(
            pluginDirectory.PluginDirectoryPath,
            pluginDirectory.SharedAssemblyDirectoryPath
        );

        Assert.HasCount(2, catalog.Plugins);
        Assert.HasCount(1, catalog.DuplicatePlugins);
        var candidates = catalog.GetAssemblyCandidates("Microsoft.Extensions.AI.Abstractions");
        Assert.HasCount(1, candidates);
        Assert.AreEqual(pluginDirectory.SharedDependencyAssemblyPath, candidates[0].Path);
    }

    [TestMethod]
    public void ConfigureServices_RegistersAlreadyLoadedPluginsInDependencyOrder()
    {
        using var pluginDirectory = TemporaryPluginDirectory.Create();
        using var loader = new PluginLoader(pluginDirectory.PluginDirectoryPath);
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration["Database:SqLite:Enable"] = "true";
        builder.Configuration["Database:SqLite:DataSource"] = ":memory:";

        loader.ConfigureServices(builder);
        builder.Services.AddServices();

        var descriptors = builder.Services.ToList();
        var coreContextIndex = descriptors.FindIndex(descriptor =>
            descriptor.ServiceType == typeof(SqlContext)
        );
        var proContextIndex = descriptors.FindIndex(descriptor =>
            descriptor.ServiceType == typeof(SqlContextPro)
        );
        Assert.IsGreaterThanOrEqualTo(0, coreContextIndex);
        Assert.IsGreaterThan(coreContextIndex, proContextIndex);

        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
        Assert.AreEqual("Microsoft.EntityFrameworkCore.Sqlite", db.Database.ProviderName);
    }

    private sealed class TemporaryPluginDirectory : IDisposable
    {
        private TemporaryPluginDirectory(
            string rootPath,
            string pluginDirectoryPath,
            string sharedAssemblyDirectoryPath,
            string sharedDependencyAssemblyPath
        )
        {
            RootPath = rootPath;
            PluginDirectoryPath = pluginDirectoryPath;
            SharedAssemblyDirectoryPath = sharedAssemblyDirectoryPath;
            SharedDependencyAssemblyPath = sharedDependencyAssemblyPath;
        }

        public string RootPath { get; }

        public string PluginDirectoryPath { get; }

        public string SharedAssemblyDirectoryPath { get; }

        public string SharedDependencyAssemblyPath { get; }

        public static TemporaryPluginDirectory Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"uzonmail-plugin-tests-{Guid.NewGuid():N}"
            );
            var pluginDirectory = System.IO.Path.Combine(path, "Plugins");
            var sharedAssemblyDirectory = System.IO.Path.Combine(path, "Assembly");
            var coreDirectory = System.IO.Path.Combine(pluginDirectory, "Core");
            var proDirectory = System.IO.Path.Combine(pluginDirectory, "Pro");
            Directory.CreateDirectory(coreDirectory);
            Directory.CreateDirectory(proDirectory);
            Directory.CreateDirectory(sharedAssemblyDirectory);

            var corePluginPath = typeof(global::UzonMail.CorePlugin.PluginSetup).Assembly.Location;
            var proPluginPath = typeof(global::UzonMail.ProPlugin.PluginSetup).Assembly.Location;
            var corePluginDirectory = System.IO.Path.GetDirectoryName(corePluginPath)!;
            var aiAbstractionsAssemblyPath = System.IO.Path.Combine(
                corePluginDirectory,
                "Microsoft.Extensions.AI.Abstractions.dll"
            );
            var sharedDependencyAssemblyPath = System.IO.Path.Combine(
                sharedAssemblyDirectory,
                System.IO.Path.GetFileName(aiAbstractionsAssemblyPath)
            );
            File.Copy(
                corePluginPath,
                System.IO.Path.Combine(coreDirectory, System.IO.Path.GetFileName(corePluginPath))
            );
            File.Copy(
                corePluginPath,
                System.IO.Path.Combine(proDirectory, System.IO.Path.GetFileName(corePluginPath))
            );
            File.Copy(
                proPluginPath,
                System.IO.Path.Combine(proDirectory, System.IO.Path.GetFileName(proPluginPath))
            );
            File.Copy(
                corePluginPath,
                System.IO.Path.Combine(
                    sharedAssemblyDirectory,
                    System.IO.Path.GetFileName(corePluginPath)
                )
            );
            File.Copy(aiAbstractionsAssemblyPath, sharedDependencyAssemblyPath);
            return new TemporaryPluginDirectory(
                path,
                pluginDirectory,
                sharedAssemblyDirectory,
                sharedDependencyAssemblyPath
            );
        }

        public void Dispose()
        {
            Directory.Delete(RootPath, true);
        }
    }
}
