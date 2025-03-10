using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Utils;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.SignalRHubs.Extensions;
using UZonMail.Core.SignalRHubs.SendEmail;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件任务后处理器
    /// 1. 若发件成功，移除成功项
    /// 2. 若发件失败，添加失败项
    /// 3. 发送消息通知
    /// </summary>
    public class GroupTaskPostHandler(GroupTasksList groupTasksList) : AbstractSendingHandler
    {
        protected override async Task HandleCore(SendingContext context)
        {
            // 判断是否有发件项，若没有，则直接返回
            var emailItem = context.EmailItem;
            if (emailItem == null)
                return;

            // 保存组的发送进度及通知前端
            if (!emailItem.IsErrorOrSuccess()) return;

            // 向数据库中保存状态
            var sqlContext = context.SqlContext;            
            var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(sqlContext, emailItem.SendingItem.SendingGroupId);

            var lastMessage = $"[{context.OutboxAddress.Email}] -> [{string.Join(",", emailItem.Inboxes.Select(x => x.Email))}]";
            sendingGroup.LastMessage = lastMessage;
            await sqlContext.SaveChangesAsync();

            // 向用户推送发送组的进度            
            var client = context.HubClient.GetUserClient(emailItem.UserId);
            // 推送发送组进度
            await client.SendingGroupProgressChanged(new SendingGroupProgressArg(sendingGroup, context.GroupTaskStartDate));

            var outbox = context.OutboxAddress;
            if (outbox == null) return;
            if (emailItem.Parent.ToSendingCount > 0) return;

            // 若是最后一封邮件，要标记办结
            if (!groupTasksList.TryGetValue(outbox.UserId, out var groupTasks)) return;
            if(groupTasks.TryRemove(sendingGroup.Id,out _))
            {
                // 标记办结
                sendingGroup.Status = SendingGroupStatus.Finish;
                sendingGroup.SendEndDate = DateTime.Now;
                await sqlContext.SaveChangesAsync();

                // 通知发件组发送完成                
                await client.SendingGroupProgressChanged(new SendingGroupProgressArg(sendingGroup, context.GroupTaskStartDate)
                {
                    ProgressType = ProgressType.End
                });
            }
        }
    }
}
