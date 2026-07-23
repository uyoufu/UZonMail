using Quartz;
using UzonMail.CorePlugin.Jobs;

namespace UzonMail.CorePlugin.Services.HostedServices
{
    /// <summary>
    /// 重置定时器
    /// </summary>
    public class ResetSchedulerService(ISchedulerFactory schedulerFactory)
        : IScopedServiceAfterStarting
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

            #region 重置每日发件限制
            var jobKey = new JobKey($"schduleTask-resetSentCountToday");
            bool exist = await scheduler.CheckExists(jobKey, stoppingToken);
            if (exist)
                await scheduler.DeleteJob(jobKey, stoppingToken);

            var job = JobBuilder.Create<SentCountReseter>().WithIdentity(jobKey).Build();
            var nextUtcMidnight = DateTime.UtcNow.Date.AddDays(1);

            var trigger = TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .StartAt(new DateTimeOffset(nextUtcMidnight, TimeSpan.Zero))
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).OnEveryDay())
                .Build();
            await scheduler.ScheduleJob(job, trigger, stoppingToken);
            #endregion
        }
    }
}
