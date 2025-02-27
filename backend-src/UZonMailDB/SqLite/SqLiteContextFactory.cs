using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using SQLitePCL;

namespace UZonMail.DB.SqLite
{
    public class SqLiteContextFactory : IDesignTimeDbContextFactory<SqLiteContext>
    {
        public SqLiteContext CreateDbContext(string[] args)
        {
            Batteries.Init();

            var optionsBuilder = new DbContextOptionsBuilder<SqlContext>();
            optionsBuilder.UseSqlite("Data Source=UZonMail/uzon-mail.db");

            return new SqLiteContext(optionsBuilder.Options);
        }
    }
}
