using Quartz;
using UzonMail.CorePlugin.Jobs;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingScheduleService(ISchedulerFactory schedulerFactory)
        : ISendingScheduleService,
            IScopedService<ISendingScheduleService>
    {
        public async Task ScheduleSending(SendingGroup sendingGroup)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var jobKey = GetJobKey(sendingGroup.Id);

            var job = JobBuilder
                .Create<EmailSendingJob>()
                .WithIdentity(jobKey)
                .SetJobData(new JobDataMap { { "sendingGroupId", sendingGroup.Id } })
                .Build();

            var scheduleDate =
                sendingGroup.ScheduleDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(sendingGroup.ScheduleDate, DateTimeKind.Local)
                    : sendingGroup.ScheduleDate;
            var trigger = TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .StartAt(new DateTimeOffset(scheduleDate))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task RemoveSendSchedule(long sendingGroupId)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.DeleteJob(GetJobKey(sendingGroupId));
        }

        private static JobKey GetJobKey(long sendingGroupId) =>
            new($"emailSending-{sendingGroupId}", "sendingGroup");
    }
}
