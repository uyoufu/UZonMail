using System.Linq;
using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.HostedServices;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.CorePlugin.Database.Initializers
{
    public class DbInitializerService(IEnumerable<IDbInitializer> dbInitializers, SqlContext db)
        : IScopedServiceAfterStarting
    {
        public int Order => -1;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 对其排序
            // 查找数据库，判断是否已经初始化
            var intializersDic = dbInitializers
                .Select(x => new KeyValuePair<string, IDbInitializer>(
                    $"DbInitializers:{x.Name}",
                    x
                ))
                .ToDictionary(x => x.Key, x => x.Value);

            var initializedNames = await db
                .AppSettings.Where(x => intializersDic.Keys.Contains(x.Key))
                .Select(x => x.Key)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var kv in intializersDic)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                if (initializedNames.Contains(kv.Key))
                {
                    continue;
                }

                // 开始执行
                await kv.Value.ExecuteAsync();

                // 保存到数据库中
                db.AppSettings.Add(new AppSetting { Key = kv.Key, BoolValue = true });
                await db.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
