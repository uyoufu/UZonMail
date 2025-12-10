using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Utils;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.SignalRHubs.Extensions;
using UZonMail.CorePlugin.SignalRHubs.SendEmail;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件任务后处理器
    /// 1. 若发件成功，移除成功项
    /// 2. 若发件失败，添加失败项
    /// 3. 发送消息通知
    /// </summary>
    public class GroupTaskPostHandler(UserGroupTasksPools userGroupTasksPools)
        : AbstractSendingHandler
    {
        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 判断是否有发件项，若没有，则直接返回
            var emailItem = context.EmailItem;
            if (emailItem == null)
                return HandlerResult.Skiped();

            // 保存组的发送进度及通知前端
            if (!emailItem.IsErrorOrSuccess())
                return HandlerResult.Skiped();

            // 向数据库中保存状态
            var sqlContext = context.SqlContext;
            var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                sqlContext,
                emailItem.SendingItem.SendingGroupId
            );

            var lastMessage =
                $"[{context.OutboxAddress.Email}] -> [{string.Join(",", emailItem.Inboxes.Select(x => x.Email))}]";
            sendingGroup.LastMessage = lastMessage;
            await sqlContext.SaveChangesAsync();

            // 向用户推送发送组的进度
            await context
                .HubClient.GetUserClient(emailItem.UserId)
                // 推送发送组进度
                .SendingGroupProgressChanged(
                    new SendingGroupProgressArg(sendingGroup, context.GroupTaskStartDate)
                );

            var outbox = context.OutboxAddress;
            if (outbox == null)
                return HandlerResult.Skiped();
            if (emailItem.Parent.ToSendingCount > 0)
                return HandlerResult.Skiped();

            // 若是最后一封邮件，要标记办结
            if (!userGroupTasksPools.TryGetValue(outbox.UserId, out var groupTasks))
                return HandlerResult.Skiped();

            if (groupTasks.TryRemove(sendingGroup.Id, out _))
            {
                var finisher = context.Provider.GetRequiredService<SendingGroupFinisher>();
                await finisher.MarkSendingGroupFinished(
                    sendingGroup.Id,
                    context.GroupTaskStartDate
                );
            }

            return HandlerResult.Success();
        }
    }
}
