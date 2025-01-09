using Quartz;
using UZonMail.DB.SQL;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Jobs;
using UZonMail.Core.Database.Init;
using UZonMail.Core.Database.Updater;
using UZonMail.Core.Config;
using UZonMail.Utils.Database.Initializer;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 程序启动时，开始中断的发件任务
    /// </summary>
    public class SendingHostedService(IServiceScopeFactory ssf) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = ssf.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            await InitDatabase(serviceProvider);
            await InitScheduler(serviceProvider);
        }

        /// <summary>
        /// 初始化化数据库
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task InitDatabase(IServiceProvider serviceProvider)
        {
            // 数据库迁移
            var context = serviceProvider.GetRequiredService<SqlContext>();
            context.Database.Migrate();
            await context.Database.EnsureCreatedAsync();

            // 数据库初始化
            var dbStartup = serviceProvider.GetRequiredService<DatabaseStartup>();
            await dbStartup.Init();

            // 数据升级
            var dataUpdater = serviceProvider.GetRequiredService<DatabaseUpdateService>();            
            await dataUpdater.Update();
        }

        /// <summary>
        /// 初始化调度器
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task InitScheduler(IServiceProvider serviceProvider)
        {
            var schdulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schdulerFactory.GetScheduler();

            #region 重置每日发件限制
            var jobKey = new JobKey($"schduleTask-resetSentCountToday");
            bool exist = await scheduler.CheckExists(jobKey);
            if (exist) return;

            var job = JobBuilder.Create<SentCountReseter>()
                .WithIdentity(jobKey)
                .Build();

            var trigger = TriggerBuilder.Create()
                .ForJob(jobKey)
                .StartAt(new DateTimeOffset(DateTime.Now.AddDays(1).Date)) // 明天凌晨开始
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).OnEveryDay())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            #endregion
        }
    }
}
