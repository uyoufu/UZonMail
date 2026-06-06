using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.HostedServices;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Database.Startup
{
    /// <summary>
    /// 初始化数据库
    /// 每次启动时，都需要执行
    /// </summary>
    /// <param name="db"></param>
    public class DatabaseReset(SqlContext db, ISendingGroupStatusService sendingGroup)
        : IScopedServiceAfterStarting
    {
        public int Order => 99;

        /// <summary>
        /// 开始执行初始化
        /// </summary>
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sendingGroupIds = await db
                .SendingGroups.Where(x => x.Status == SendingGroupStatus.Sending)
                .Select(x => x.Id)
                .ToListAsync();

            await sendingGroup.UpdateSendingGroupStatus(
                sendingGroupIds,
                SendingGroupStatus.Pause,
                "系统重启导致暂停"
            );
        }
    }
}
