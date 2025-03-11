using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.DB.Utils;

namespace UZonMail.Core.Database.Startup
{
    /// <summary>
    /// 初始化数据库
    /// 每次启动时，都需要执行
    /// </summary>
    /// <param name="db"></param>
    public class DatabaseSetup(SqlContext db) : IScopedService
    {
        /// <summary>
        /// 开始执行初始化
        /// </summary>
        public async Task Start()
        {
            await ResetSendingGroup();
            await ResetSendingItemsStatus();
        }

        private async Task ResetSendingGroup()
        {
            // 将所有的 Sending 或者 Create 状态的发件组重置为 Finish
            await db.SendingGroups.UpdateAsync(x => x.Status == SendingGroupStatus.Sending
                || x.Status == SendingGroupStatus.Created
            , obj => obj.SetProperty(x => x.Status, SendingGroupStatus.Finish)
                .SetProperty(x => x.LastMessage, "系统被中断")
            );
        }

        private async Task ResetSendingItemsStatus()
        {
            // 对所有的 Pending 状态的发件项重置为 Created
            await db.SendingItems.UpdateAsync(x => x.Status == SendingItemStatus.Pending || x.Status == SendingItemStatus.Sending,
                x => x.SetProperty(y => y.Status, SendingItemStatus.Created));
        }
    }
}
