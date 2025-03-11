using Microsoft.EntityFrameworkCore;
using UZonMail.DB.MySql;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.SQL
{
    public class SqlContextHelper
    {
        /// <summary>
        /// 配置 sqlite
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public static void ConfiguringSqLite(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            if (options.IsConfigured) return;

            // sqlLite
            var sqLiteConnectionConfig = new SqLiteConnectionConfig();
            configuration.GetSection("Database:SqLite").Bind(sqLiteConnectionConfig);

            var sqlLiteFilePath = sqLiteConnectionConfig.DataSource;
            if (!string.IsNullOrEmpty(sqlLiteFilePath))
            {
                var directory = Path.GetDirectoryName(sqlLiteFilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            }
            options.UseSqlite(sqLiteConnectionConfig.ConnectionString);
        }

        /// <summary>
        /// 配置 mysql
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public static void ConfiguringMySql(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            if (options.IsConfigured) return;

            var _mysqlConnectionConfig = new MySqlConnectionConfig();
            configuration.GetSection("Database:MySql").Bind(_mysqlConnectionConfig);
            options.UseMySql(_mysqlConnectionConfig.ConnectionString, new MySqlServerVersion(_mysqlConnectionConfig.MysqlVersion));
        }
    }
}
