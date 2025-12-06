using Quartz;
using UZonMail.CorePlugin.Utils.Database;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Jobs
{
    /// <summary>
    /// 发送计数重置任务
    /// </summary>
    public class SentCountReseter(SqlContext db) : IJob, IScopedService
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // 重置数据库
            await db.Outboxes.UpdateAsync(
                x => true,
                x => x.SetProperty(y => y.MaxSendCountPerDay, 0)
            );

            // 发件池在调用时，会自动重置，此处不处理
            return;
        }
    }
}
