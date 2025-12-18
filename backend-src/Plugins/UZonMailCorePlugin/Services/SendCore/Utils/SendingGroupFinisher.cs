using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.Notification.EmailNotifier;
using UZonMail.CorePlugin.SignalRHubs;
using UZonMail.CorePlugin.SignalRHubs.Extensions;
using UZonMail.CorePlugin.SignalRHubs.SendEmail;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Utils
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
