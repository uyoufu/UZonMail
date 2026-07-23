using LiteDB;
using UzonMail.CorePlugin.Config.SubConfigs;

namespace UzonMail.CorePlugin.Utils.Database
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
