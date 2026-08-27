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

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

/// <summary>
/// 清理退出发件箱关联的待发项，并更新受影响发送组的状态和通知。
/// </summary>
public sealed class SenderAccountLinkedGroupCleanupService(
    GroupTasksManager groupTasksManager,
    SenderAccountsManager senderAccountsManager,
    SqlContext db,
    IHubContext<UzonMailHub, IUzonMailClient> hub,
    SendingGroupFinisher sendingGroupFinisher
) : IScopedService
{
    /// <summary>
    /// 清理发件箱关联的待发项，并通知受影响的发送组。
    /// </summary>
    public async Task CleanupAsync(SenderEmailAddress senderAccount, DateTime groupTaskStartDate)
    {
        if (!groupTasksManager.TryGetUserTasksPool(senderAccount.UserId, out var groupTasks))
            return;

        var client = hub.GetUserClient(senderAccount.UserId);
        foreach (var sendingGroupId in senderAccount.GetSendingGroupIds())
        {
            if (!groupTasks!.TryGetValue(sendingGroupId, out var groupTask))
                continue;

            if (senderAccountsManager.ExistValidSenderAccount(sendingGroupId))
            {
                var sendingItemIds = senderAccount.GetSpecificSendingItemIds(sendingGroupId);
                groupTask.RemovePendingItems(sendingItemIds);
                await FailSpecificItemsAsync(
                    sendingGroupId,
                    sendingItemIds,
                    senderAccount.ErroredMessage
                );
                var sendingGroup = await SendingGroupUpdater.UpdateSendingGroupSentInfo(
                    db,
                    sendingGroupId
                );
                await client.SendingGroupProgressChanged(
                    new SendingGroupProgressArg(sendingGroup, groupTaskStartDate)
                );
                continue;
            }

            await groupTasksManager.RemoveSendingGroupTaskAsync(
                senderAccount.UserId,
                sendingGroupId
            );
            await FailAllPendingItemsAsync(sendingGroupId, senderAccount.ErroredMessage);
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
