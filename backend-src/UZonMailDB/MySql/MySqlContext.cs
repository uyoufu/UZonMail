using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UZonMail.DB.SQL;

namespace UZonMail.DB.MySql
{
    public class MySqlContext : SqlContext
    {
        private readonly MySqlConnectionConfig _mysqlConnectionConfig;

        internal MySqlContext(DbContextOptions options) : base(options)
        {
        }

        public MySqlContext(IConfiguration configuration)
        {
            _mysqlConnectionConfig = new();
            configuration.GetSection("Database:MySql").Bind(_mysqlConnectionConfig);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (_mysqlConnectionConfig != null)
                options.UseMySql(_mysqlConnectionConfig.ConnectionString, new MySqlServerVersion(_mysqlConnectionConfig.MysqlVersion));
        }
    }
}
