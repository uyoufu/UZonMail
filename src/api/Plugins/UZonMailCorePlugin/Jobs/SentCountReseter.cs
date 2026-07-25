using Microsoft.EntityFrameworkCore;
using Quartz;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Jobs
{
    /// <summary>
    /// 发送计数重置任务
    /// </summary>
    public class SentCountReseter(SqlContext db, ISendingGroupCommandService commandService)
        : IJob,
            IScopedService
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // 计数按日期惰性重置，避免每天更新整张发件箱表；这里只恢复额度等待组。
            var utcNow = DateTime.UtcNow;
            var waitingGroups = await db
                .SendingGroups.AsNoTracking()
                .Where(group =>
                    group.Status == SendingGroupStatus.WaitingForQuotaReset
                    && group.ResumeAtUtc <= utcNow
                )
                .OrderBy(group => group.Id)
                .ToListAsync(context.CancellationToken);

            foreach (var sendingGroup in waitingGroups)
                await commandService.SendNow(sendingGroup);
        }
    }
}
