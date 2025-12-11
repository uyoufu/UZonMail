using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.Utils;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.SignalRHubs.Extensions;
using UZonMail.CorePlugin.SignalRHubs.SendEmail;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class OutboxDisposer(GroupTasksManager groupTasksManager, OutboxesManager outboxManager)
        : AbstractSendingHandler
    {
        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            var outbox = context.OutboxAddress;
            if (outbox != null && outbox.ShouldDispose)
            {
                // 从发件箱池中移除
                outboxManager.RemoveOutbox(outbox, outbox.ErroredMessage);

                // 移除对应的发件组中的数据
                // 1. 特定发件箱，移除特定邮件
                // 2. 共享发件箱，判断是否还有多余的发件箱，若没有，则整体移除
                await RemoveLinkingGroups(context, outbox);
            }

            return HandlerResult.Success();
        }

        /// <summary>
        /// 移除关联的发件组
        /// 这个方法必须在 outbox 被移除后，才能调用
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="outbox"></param>
        /// <returns></returns>
        private async Task RemoveLinkingGroups(
            SendingContext sendingContext,
            OutboxEmailAddress outbox
        )
        {
            // 受影响的发件任务
            if (!groupTasksManager.TryGetUserTasksPool(outbox.UserId, out var groupTasks))
                return;

            var sqlContext = sendingContext.SqlContext;
            var hub = sendingContext.HubClient;
            var client = hub.GetUserClient(outbox.UserId);

            // 移除指定发件箱的发件项
            var sendingGroupIds = outbox.GetSendingGroupIds();
            foreach (var sendingGroupId in sendingGroupIds)
            {
                // 获取发件组
                if (!groupTasks.TryGetValue(sendingGroupId, out var groupTask))
                    continue;

                // 判断当前发件组是否还有发件箱
                var isExistValidOutbox = outboxManager.ExistValidOutbox(sendingGroupId);
                if (isExistValidOutbox)
                {
                    var sendingItemIds = outbox.GetSpecificSendingItemIds();

                    // 从发件组中移除发件箱
                    groupTask.RemovePendingItems(sendingItemIds);

                    // 标记为错误
                    await sqlContext.SendingItems.UpdateAsync(
                        x => x.SendingGroupId == sendingGroupId && sendingItemIds.Contains(x.Id),
                        x =>
                            x.SetProperty(y => y.Status, SendingItemStatus.Failed)
                                .SetProperty(y => y.SendDate, DateTime.UtcNow)
                                .SetProperty(
                                    y => y.SendResult,
                                    outbox.ErroredMessage ?? "发件箱退出发件池，无发件箱可用"
                                )
                    );

                    // 更新发件组成功的数据
                    var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                        sqlContext,
                        sendingGroupId
                    );
                    // 推送发送组进度
                    await client.SendingGroupProgressChanged(
                        new SendingGroupProgressArg(sendingGroup, sendingContext.GroupTaskStartDate)
                    );
                    continue;
                }

                // 发件组不存在任何发件箱时，需要移除整个发件组
                groupTasksManager.RemoveSendingGroupTask(outbox.UserId, sendingGroupId);

                // 修改发件项状态
                await sendingContext.SqlContext.SendingItems.UpdateAsync(
                    x =>
                        x.SendingGroupId == sendingGroupId && x.Status == SendingItemStatus.Pending,
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Failed)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(
                                y => y.SendResult,
                                outbox.ErroredMessage ?? "发件箱退出发件池，无发件箱可用"
                            )
                );

                // 更新发件组成功的数据
                var removedSendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                    sqlContext,
                    sendingGroupId
                );

                // 标记结束
                var finisher = sendingContext.Provider.GetRequiredService<SendingGroupFinisher>();
                await finisher.MarkSendingGroupFinished(
                    removedSendingGroup.Id,
                    sendingContext.GroupTaskStartDate
                );
            }
        }
    }
}
