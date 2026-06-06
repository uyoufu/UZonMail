using Quartz;
using UzonMail.CorePlugin.Utils.Database;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Jobs
{
    /// <summary>
    /// 发送计数重置任务
    /// </summary>
    public class SentCountReseter(SqlContext db) : IJob, IScopedService
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // 重置每日已发送计数，不应改动用户配置的每日最大发送数。
            await db.Outboxes.UpdateAsync(x => true, x => x.SetProperty(y => y.SentTotalToday, 0));

            // 发件池在调用时，会自动重置，此处不处理
            return;
        }
    }
}
