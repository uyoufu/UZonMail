using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.MySql
{
    public class MySqlContextFactory : IDesignTimeDbContextFactory<MySqlContext>
    {
        public MySqlContext CreateDbContext(string[] args)
        {
            Batteries.Init();

            var connection = new MySqlConnectionConfig()
            {
                Database = "uzon-mail",
                Enable = true,
                Host = "",
                Password = "uzon-mail",
                Port = 3306,
                Version = "8.4.0.0",
                User = "uzon-mail"
            };

            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseMySql(connection.ConnectionString, new MySqlServerVersion(connection.MysqlVersion));

            return new MySqlContext(optionsBuilder.Options);
        }
    }
}
