using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UzonMail.DB.SQL;
using UzonMail.Utils.Web.Service;

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
