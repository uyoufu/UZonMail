using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMailService.Services.PostStartup;

namespace UZonMail.Core.Services.HostedServices
{
    public class SqlContextMigration(SqlContext db) : IHostedServiceStart, IScopedService<IHostedServiceStart>
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
