using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Notification.EmailNotifier;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Utils
{
    public class SendingGroupFinisher(
        SqlContext db,
        IHubContext<UzonMailHub, IUzonMailClient> hubContext,
        SendingGroupFinishedNotification notification
    ) : IScopedService
    {
        public async Task SetSendingGroupStatusAndNotify(
            long sendingGroupId,
            SendingGroupStatus status,
            DateTime groupTaskStartDate
        )
        {
            var sendingGroup = await db.SendingGroups.FirstAsync(x => x.Id == sendingGroupId);
            // 标记办结
            sendingGroup.Status = status;
            sendingGroup.SendEndDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // 通知发件组发送完成
            await hubContext
                .GetUserClient(sendingGroup.UserId)
                .SendingGroupProgressChanged(
                    new SendingGroupProgressArg(sendingGroup, groupTaskStartDate)
                    {
                        ProgressType = ProgressType.End
                    }
                );

            await notification.Notify(sendingGroup);
        }
    }
}
