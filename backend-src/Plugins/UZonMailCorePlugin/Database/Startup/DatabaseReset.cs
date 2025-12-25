using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.HostedServices;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Database.Startup
{
    /// <summary>
    /// 初始化数据库
    /// 每次启动时，都需要执行
    /// </summary>
    /// <param name="db"></param>
    public class DatabaseReset(SqlContext db, SendingGroupService sendingGroup)
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
