using LiteDB;
using UZonMail.CorePlugin.Config.SubConfigs;

namespace UZonMail.CorePlugin.Utils.Database
{
    /// <summary>
    /// LiteDB管理器
    /// </summary>
    /// <remarks>
    /// 数据库操作
    /// </remarks>
    public class LiteDBContext(IConfiguration configuration)
        : LiteRepository(
            new ConnectionString()
            {
                Filename = configuration.GetValue<string>(DatabaseConfig.GetLiteDbPathConfigKey()),
                Upgrade = true
            },
            new SMEBsonMapper()
        ) { }
}
