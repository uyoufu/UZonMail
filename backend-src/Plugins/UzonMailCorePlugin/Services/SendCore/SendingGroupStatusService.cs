using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingGroupStatusService(SqlContext db)
        : ISendingGroupStatusService,
            IScopedService<ISendingGroupStatusService>
    {
        public async Task UpdateSendingGroupStatus(
            long sendingGroupId,
            SendingGroupStatus status,
            string updateReason = ""
        )
        {
            await UpdateSendingGroupStatus([sendingGroupId], status, updateReason);
        }

        public async Task UpdateSendingGroupStatus(
            List<long> sendingGroupIds,
            SendingGroupStatus status,
            string updateReason = ""
        )
        {
            if (sendingGroupIds.Count == 0)
                return;

            var sendingItemStatus = SendingGroupStatusMapper.ToSendingItemStatus(status);

            await db.SendingGroups.UpdateAsync(
                x => sendingGroupIds.Contains(x.Id),
                x => x.SetProperty(y => y.Status, status)
            );
            await db.SendingItems.UpdateAsync(
                x =>
                    sendingGroupIds.Contains(x.SendingGroupId)
                    && (
                        x.Status == SendingItemStatus.Pending
                        || x.Status == SendingItemStatus.Sending
                    ),
                x =>
                    x.SetProperty(y => y.Status, sendingItemStatus)
                        .SetProperty(y => y.SendResult, updateReason)
            );
        }
    }
}
