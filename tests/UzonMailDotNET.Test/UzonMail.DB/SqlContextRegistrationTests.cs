using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UzonMail.CorePlugin;
using UzonMail.DB.PostgreSql;
using UzonMail.DB.SQL;
using UzonMail.DB.SqLite;
using UzonMail.Utils.Web;

namespace UzonMailDotNET.Test.UzonMail.DB;

/// <summary>
/// 验证插件数据库注册不会被后续自动服务扫描覆盖。
/// </summary>
[TestClass]
public sealed class SqlContextRegistrationTests
{
    [TestMethod]
    public void CorePlugin_WithSqlite_ResolvesConfiguredSqlContextAfterServiceScan()
    {
        var builder = CreateHostBuilder(
            new Dictionary<string, string?>
            {
                ["Database:SqLite:Enable"] = "true",
                ["Database:SqLite:DataSource"] = ":memory:",
            }
        );
        new PluginSetup().ConfigureServices(builder);

        builder.Services.AddServices();

        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
        Assert.IsInstanceOfType<SqLiteContext>(db);
        Assert.AreEqual("Microsoft.EntityFrameworkCore.Sqlite", db.Database.ProviderName);
    }

    [TestMethod]
    public void CorePlugin_PrefersPostgreSqlWhenBothProvidersAreEnabled()
    {
        var builder = CreateHostBuilder(
            new Dictionary<string, string?>
            {
                ["Database:PostgreSql:Enable"] = "true",
                ["Database:PostgreSql:Host"] = "127.0.0.1",
                ["Database:PostgreSql:Port"] = "5432",
                ["Database:PostgreSql:Database"] = "uzon-mail-test",
                ["Database:PostgreSql:User"] = "test",
                ["Database:PostgreSql:Password"] = "test",
                ["Database:SqLite:Enable"] = "true",
                ["Database:SqLite:DataSource"] = ":memory:",
            }
        );
        new PluginSetup().ConfigureServices(builder);

        builder.Services.AddServices();

        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
        Assert.IsInstanceOfType<PostgreSqlContext>(db);
        Assert.AreEqual("Npgsql.EntityFrameworkCore.PostgreSQL", db.Database.ProviderName);
    }

    private static HostApplicationBuilder CreateHostBuilder(
        IReadOnlyDictionary<string, string?> configurationValues
    )
    {
        var settings = new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            Configuration = new ConfigurationManager(),
        };
        var builder = new HostApplicationBuilder(settings);
        builder.Configuration.AddInMemoryCollection(configurationValues);
        return builder;
    }
}
