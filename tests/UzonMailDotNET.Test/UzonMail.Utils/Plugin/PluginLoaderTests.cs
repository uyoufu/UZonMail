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

        var catalog = PluginAssemblyCatalog.Create(pluginDirectory.Path);
        var sortedPlugins = PluginDependencySorter.Sort(catalog.Plugins);

        Assert.HasCount(2, sortedPlugins);
        Assert.AreEqual("UzonMail.CorePlugin", sortedPlugins[0].Name);
        Assert.AreEqual("UzonMail.ProPlugin", sortedPlugins[1].Name);
        Assert.HasCount(1, catalog.DuplicatePlugins);
        Assert.AreEqual("UzonMail.CorePlugin", catalog.DuplicatePlugins[0].AssemblyName);
    }

    [TestMethod]
    public void ConfigureServices_RegistersAlreadyLoadedPluginsInDependencyOrder()
    {
        using var pluginDirectory = TemporaryPluginDirectory.Create();
        using var loader = new PluginLoader(pluginDirectory.Path);
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
        private TemporaryPluginDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryPluginDirectory Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"uzonmail-plugin-tests-{Guid.NewGuid():N}"
            );
            var coreDirectory = System.IO.Path.Combine(path, "Core");
            var proDirectory = System.IO.Path.Combine(path, "Pro");
            Directory.CreateDirectory(coreDirectory);
            Directory.CreateDirectory(proDirectory);

            var corePluginPath = typeof(global::UzonMail.CorePlugin.PluginSetup).Assembly.Location;
            var proPluginPath = typeof(global::UzonMail.ProPlugin.PluginSetup).Assembly.Location;
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
            return new TemporaryPluginDirectory(path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}
