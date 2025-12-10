using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Utils.Database;
using UZonMail.DB.Extensions;
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
    public class DatabaseReset(SqlContext db, SendingGroupService sendingGroup) : IScopedService
    {
        /// <summary>
        /// 开始执行初始化
        /// </summary>
        public async Task Start()
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
