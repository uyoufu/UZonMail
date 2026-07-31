using Microsoft.AspNetCore.SignalR;
using UzonMail.CorePlugin.Services.SendCore.Utils;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Outboxes;

/// <summary>
/// 清理退出发件箱关联的待发项，并更新受影响发送组的状态和通知。
/// </summary>
public sealed class OutboxLinkedGroupCleanupService(
    GroupTasksManager groupTasksManager,
    OutboxesManager outboxesManager,
    SqlContext db,
    IHubContext<UzonMailHub, IUzonMailClient> hub,
    SendingGroupFinisher sendingGroupFinisher
) : IScopedService
{
    /// <summary>
    /// 清理发件箱关联的待发项，并通知受影响的发送组。
    /// </summary>
    public async Task CleanupAsync(OutboxEmailAddress outbox, DateTime groupTaskStartDate)
    {
        if (!groupTasksManager.TryGetUserTasksPool(outbox.UserId, out var groupTasks))
            return;

        var client = hub.GetUserClient(outbox.UserId);
        foreach (var sendingGroupId in outbox.GetSendingGroupIds())
        {
            if (!groupTasks!.TryGetValue(sendingGroupId, out var groupTask))
                continue;

            if (outboxesManager.ExistValidOutbox(sendingGroupId))
            {
                var sendingItemIds = outbox.GetSpecificSendingItemIds(sendingGroupId);
                groupTask.RemovePendingItems(sendingItemIds);
                await FailSpecificItemsAsync(sendingGroupId, sendingItemIds, outbox.ErroredMessage);
                var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                    db,
                    sendingGroupId
                );
                await client.SendingGroupProgressChanged(
                    new SendingGroupProgressArg(sendingGroup, groupTaskStartDate)
                );
                continue;
            }

            await groupTasksManager.RemoveSendingGroupTaskAsync(outbox.UserId, sendingGroupId);
            await FailAllPendingItemsAsync(sendingGroupId, outbox.ErroredMessage);
            var removedSendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                db,
                sendingGroupId
            );
            await sendingGroupFinisher.SetSendingGroupStatusAndNotify(
                removedSendingGroup.Id,
                SendingGroupStatus.Pause,
                groupTaskStartDate
            );
        }
    }

    private Task FailSpecificItemsAsync(
        long sendingGroupId,
        IReadOnlyCollection<long> sendingItemIds,
        string? message
    )
    {
        if (sendingItemIds.Count == 0)
            return Task.CompletedTask;

        return db.SendingItems.UpdateAsync(
            x => x.SendingGroupId == sendingGroupId && sendingItemIds.Contains(x.Id),
            x =>
                x.SetProperty(y => y.Status, SendingItemStatus.Failed)
                    .SetProperty(y => y.SendDate, DateTime.UtcNow)
                    .SetProperty(y => y.SendResult, GetFailureMessage(message))
        );
    }

    private Task FailAllPendingItemsAsync(long sendingGroupId, string? message)
    {
        return db.SendingItems.UpdateAsync(
            x => x.SendingGroupId == sendingGroupId && x.Status == SendingItemStatus.Pending,
            x =>
                x.SetProperty(y => y.Status, SendingItemStatus.Failed)
                    .SetProperty(y => y.SendDate, DateTime.UtcNow)
                    .SetProperty(y => y.SendResult, GetFailureMessage(message))
        );
    }

    private static string GetFailureMessage(string? message) =>
        string.IsNullOrWhiteSpace(message) ? "发件箱退出发件池，无发件箱可用" : message;
}
