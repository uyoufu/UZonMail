using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;

namespace UzonMail.CorePlugin.Services.HostedServices
{
    public class SqlContextMigration(SqlContext db) : IScopedServiceAfterStarting
    {
        public int Order => -10000;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 数据库迁移
            db.Database.Migrate();
            await db.Database.EnsureCreatedAsync(stoppingToken);
        }
    }
}
