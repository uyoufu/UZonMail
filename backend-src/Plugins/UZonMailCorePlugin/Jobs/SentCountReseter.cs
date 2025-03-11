using Quartz;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.Extensions;

namespace UZonMail.Core.Jobs
{
    /// <summary>
    /// 发送计数重置任务
    /// </summary>
    public class SentCountReseter(SqlContext db) : IJob, IScopedService
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // 重置数据库
            await db.Outboxes.UpdateAsync(x => true, x => x.SetProperty(y => y.MaxSendCountPerDay, 0));

            // 发件池在调用时，会自动重置，此处不处理
            return;
        }
    }
}
