using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Services.Notification.EmailNotification;
using UZonMail.Core.SignalRHubs;
using UZonMail.Core.SignalRHubs.Extensions;
using UZonMail.Core.SignalRHubs.SendEmail;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Utils
{
    public class SendingGroupFinisher(SqlContext db,IHubContext<UzonMailHub, IUzonMailClient> hubContext, SendingGroupFinishedNotification notification) : IScopedService
    {
        public async Task MarkSendingGroupFinished(long sendingGroupId,DateTime groupTaskStartDate)
        {
            var sendingGroup = await db.SendingGroups.FirstAsync(x=>x.Id == sendingGroupId);
            // 标记办结
            sendingGroup.Status = SendingGroupStatus.Finish;
            sendingGroup.SendEndDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // 通知发件组发送完成       
            var client = hubContext.GetUserClient(sendingGroup.UserId);
            if (client != null) {                        
                await client.SendingGroupProgressChanged(new SendingGroupProgressArg(sendingGroup, groupTaskStartDate)
                {
                    ProgressType = ProgressType.End
                });
            }

            await notification.Notify(sendingGroup);            
        }
    }
}
