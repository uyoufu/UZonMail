using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using SQLitePCL;

namespace UZonMail.DB.SqLite
{
    /// <summary>
    /// 设计时创建 DbContext 实例通。常用于 Entity Framework Core 的工具（如迁移工具），以便在没有运行时配置的情况下生成或更新数据库架构。
    /// </summary>
    public class SqLiteContextFactory : IDesignTimeDbContextFactory<SqLiteContext>
    {
        public SqLiteContext CreateDbContext(string[] args)
        {
            Console.WriteLine("SqLiteContextFactory running...");
            Batteries.Init();

            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlite("Data Source=UZonMail/uzon-mail.db");

            return new SqLiteContext(optionsBuilder.Options);
        }
    }
}
