using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.DB.PostgreSql
{
    /// <summary>
    /// 设计时创建 DbContext 实例通。常用于 Entity Framework Core 的工具（如迁移工具），以便在没有运行时配置的情况下生成或更新数据库架构。
    /// </summary>
    public class PostgreSqlContextFactory : IDesignTimeDbContextFactory<PostgreSqlContext>
    {
        public PostgreSqlContext CreateDbContext(string[] args)
        {
            Batteries.Init();

            var connection = new PostgreSqlConnectionConfig()
            {
                Database = "uzon-mail",
                Enable = true,
                Host = "127.0.0.1",
                Password = "uzon-mail",
                Port = 5432,
                User = "uzon-mail"
            };

            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseNpgsql(connection.ConnectionString);

            return new PostgreSqlContext(optionsBuilder.Options);
        }
    }
}
