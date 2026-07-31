using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Utils;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件任务后处理器
    /// 1. 若发件成功，移除成功项
    /// 2. 若发件失败，添加失败项
    /// 3. 发送消息通知
    /// </summary>
    public class GroupTaskUpdateHandler(GroupTasksManager groupTasksManager)
        : AbstractSendingHandler
    {
        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 判断是否有发件项，若没有，则直接返回
            var currentAttempt = context.CurrentAttempt;
            if (currentAttempt == null)
                return HandlerResult.Skiped();

            // 保存组的发送进度及通知前端
            if (context.SendAttemptDecision is not { IsTerminal: true })
                return HandlerResult.Skiped();

            // 向数据库中保存状态
            var sqlContext = context.SqlContext;
            var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                sqlContext,
                currentAttempt.Descriptor.SendingGroupId
            );

            var lastMessage =
                $"[{context.OutboxAddress!.Email}] -> [{string.Join(",", currentAttempt.PreparedItem.Inboxes.Select(x => x.Email))}]";
            sendingGroup.LastMessage = lastMessage;
            await sqlContext.SaveChangesAsync();

            // 向用户推送发送组的进度
            await context
                .HubClient.GetUserClient(currentAttempt.PreparedItem.UserId)
                // 推送发送组进度
                .SendingGroupProgressChanged(
                    new SendingGroupProgressArg(sendingGroup, context.GroupTaskStartDate)
                );

            var outbox = context.OutboxAddress;
            if (outbox == null)
                return HandlerResult.Skiped();

            // 判断是否还有待发送的邮件，若有，则直接返回
            if (context.GroupTask is { ShouldDispose: false })
                return HandlerResult.Skiped();

            // 若是最后一封邮件，要标记办结
            if (await groupTasksManager.RemoveSendingGroupTaskAsync(outbox.UserId, sendingGroup.Id))
            {
                var finisher = context.Provider.GetRequiredService<SendingGroupFinisher>();
                await finisher.SetSendingGroupStatusAndNotify(
                    sendingGroup.Id,
                    SendingGroupStatus.Finish,
                    context.GroupTaskStartDate
                );
            }

            return HandlerResult.Success();
        }
    }
}
