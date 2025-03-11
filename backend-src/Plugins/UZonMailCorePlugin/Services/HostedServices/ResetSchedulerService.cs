using Microsoft.Extensions.DependencyInjection;
using Quartz;
using UZonMail.Core.Jobs;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 重置定时器
    /// </summary>
    public class ResetSchedulerService(ISchedulerFactory schedulerFactory) : IHostedServiceStart, IScopedService<IHostedServiceStart>
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

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
            await scheduler.ScheduleJob(job, trigger, stoppingToken);
            #endregion
        }
    }
}
