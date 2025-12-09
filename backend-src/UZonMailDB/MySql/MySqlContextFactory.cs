using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SQLitePCL;
using UZonMail.DB.SQL;

namespace UZonMail.DB.MySql
{
    public class MySqlContextFactory : IDesignTimeDbContextFactory<MySqlContext>
    {
        public MySqlContext CreateDbContext(string[] args)
        {
            Batteries.Init();

            // 可以尝试使用 IDbContextFactory<SqlContext> dbFactory 创建 sqlContext

            var connection = new MySqlConnectionConfig()
            {
                Database = "uzon-mail",
                Enable = true,
                Host = "127.0.0.1",
                Password = "uzon-mail",
                Port = 3306,
                Version = "8.4.0.0",
                User = "uzon-mail"
            };

            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseMySql(
                connection.ConnectionString,
                new MySqlServerVersion(connection.MysqlVersion)
            );

            return new MySqlContext(optionsBuilder.Options);
        }
    }
}
