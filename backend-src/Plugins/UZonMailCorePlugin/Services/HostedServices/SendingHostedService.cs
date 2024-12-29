using Quartz;
using UZonMail.DB.SQL;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Jobs;
using UZonMail.Core.Database.Init;
using UZonMail.Core.Database.Updater;
using UZonMail.Core.Config;

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
            var nv = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var context = serviceProvider.GetRequiredService<SqlContext>();
            // 应用迁移
            context.Database.Migrate();
            await context.Database.EnsureCreatedAsync();

            var appConfig = serviceProvider.GetRequiredService<IOptions<AppConfig>>();

            // 数据初始化
            var initializer = new DatabaseInitializer(nv, context, appConfig.Value);
            await initializer.Init();

            // 数据升级
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var dataUpdater = new DataUpdaterManager(context, config);
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
