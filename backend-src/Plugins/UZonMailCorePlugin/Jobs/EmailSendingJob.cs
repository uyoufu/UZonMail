using Microsoft.EntityFrameworkCore;
using Quartz;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL;
using log4net;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Core.Services.SendCore;

namespace UZonMail.Core.Jobs
{
    /// <summary>
    /// 邮件发送定时任务
    /// </summary>
    public class EmailSendingJob(SqlContext db, SendingGroupService sendingService) : IJob, IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailSendingJob));

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Info("邮件发送定时任务开始执行");

            // 获取组 id
            var sendingGroupId = context.JobDetail.JobDataMap.GetInt("sendingGroupId");
            if (sendingGroupId == 0)
            {
                _logger.Warn(" 邮件发送定时任务需传入发送组 id");
                return;
            }

            var sendingGroup = await db.SendingGroups.Where(x => x.Id == sendingGroupId
                    && x.SendingType == SendingGroupType.Scheduled)
                .Include(x => x.Outboxes)
                .Include(x => x.Templates)
                .FirstOrDefaultAsync();

            // 添加到立即发送队列
            if (sendingGroup == null)
            {
                _logger.Warn("未能匹配到发件计划");
                return;
            }

            // 获取密钥
            string[] smtpPasswordSecretKeys = context.JobDetail.JobDataMap.GetString("smtpPasswordSecretKeys").Split(',');
            sendingGroup.SmtpPasswordSecretKeys = smtpPasswordSecretKeys.ToList();
            await sendingService.SendNow(sendingGroup);
        }
    }
}
