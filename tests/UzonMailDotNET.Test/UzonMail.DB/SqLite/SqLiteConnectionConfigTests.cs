using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using UzonMail.DB.SqLite;

namespace UzonMailDotNET.Test.UzonMail.DB.SqLite;

/// <summary>
/// 验证 SQLite 配置绑定和连接字符串生成行为。
/// </summary>
[TestClass]
public sealed class SqLiteConnectionConfigTests
{
    /// <summary>
    /// 验证未提供可选参数时使用驱动默认值。
    /// </summary>
    [TestMethod]
    public void ConnectionString_WithoutOptionalConfiguration_UsesDriverDefaults()
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["Database:SqLite:Enable"] = "true",
            ["Database:SqLite:DataSource"] = "data/db/uzon-mail.db",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        var connectionConfig = new SqLiteConnectionConfig();

        configuration.GetSection("Database:SqLite").Bind(connectionConfig);
        var connectionStringBuilder = new SqliteConnectionStringBuilder(
            connectionConfig.ConnectionString
        );

        Assert.IsTrue(connectionConfig.Enable);
        Assert.AreEqual("data/db/uzon-mail.db", connectionStringBuilder.DataSource);
        Assert.AreEqual(SqliteOpenMode.ReadWriteCreate, connectionStringBuilder.Mode);
        Assert.AreEqual(SqliteCacheMode.Default, connectionStringBuilder.Cache);
        Assert.AreEqual(string.Empty, connectionStringBuilder.Password);
    }

    /// <summary>
    /// 验证配置的打开模式、缓存模式和密码会写入连接字符串。
    /// </summary>
    [TestMethod]
    public void ConnectionString_WithOptionalConfiguration_PreservesConfiguredValues()
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["Database:SqLite:DataSource"] = "data/db/readonly.db",
            ["Database:SqLite:Mode"] = "ReadOnly",
            ["Database:SqLite:Cache"] = "Shared",
            ["Database:SqLite:Password"] = "database-password",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        var connectionConfig = new SqLiteConnectionConfig();

        configuration.GetSection("Database:SqLite").Bind(connectionConfig);
        var connectionStringBuilder = new SqliteConnectionStringBuilder(
            connectionConfig.ConnectionString
        );

        Assert.AreEqual(SqliteOpenMode.ReadOnly, connectionStringBuilder.Mode);
        Assert.AreEqual(SqliteCacheMode.Shared, connectionStringBuilder.Cache);
        Assert.AreEqual("database-password", connectionStringBuilder.Password);
    }

    /// <summary>
    /// 验证非法枚举配置会在绑定阶段失败，而不是静默使用默认值。
    /// </summary>
    [TestMethod]
    public void ConfigurationBinding_WithInvalidMode_ThrowsInvalidOperationException()
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["Database:SqLite:Mode"] = "InvalidMode",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        var connectionConfig = new SqLiteConnectionConfig();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => configuration.GetSection("Database:SqLite").Bind(connectionConfig)
        );
    }
}
