using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 提交发送结果，或将可重试邮件重新放入延迟队列。
/// </summary>
public sealed class EmailItemUpdateHandler(TimeProvider timeProvider) : AbstractSendingHandler
{
    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        var currentAttempt = context.CurrentAttempt;
        var transportResult = context.TransportResult;
        if (currentAttempt == null || transportResult == null)
            return HandlerResult.Skiped();

        var decision = SendAttemptDecisionPolicy.Decide(
            transportResult,
            currentAttempt.Descriptor.TriedCount,
            currentAttempt.PreparedItem.MaxRetryCount,
            context.OutboxRetirement
        );
        context.SendAttemptDecision = decision;

        if (decision.Disposition == SendAttemptDisposition.Release)
        {
            if (context.GroupTask?.ReleaseEmailItem(currentAttempt) != true)
                throw new InvalidOperationException(
                    $"发件项 {currentAttempt.Descriptor.Id} 无法释放回待发队列"
                );
            context.RequestWorkerExit();
            return HandlerResult.Skiped(decision.Message);
        }

        if (decision.Disposition == SendAttemptDisposition.Retry)
        {
            await ScheduleRetryAsync(context, currentAttempt, decision.Message);
            context.RequestWorkerExit();
            return HandlerResult.Skiped(decision.Message);
        }

        var sendingItem = await SaveSendingItemAsync(context, currentAttempt, decision);
        context.GroupTask?.CompleteEmailItem(currentAttempt);
        await context
            .HubClient.GetUserClient(currentAttempt.PreparedItem.Outbox.UserId)
            .SendingItemStatusChanged(new SendingItemStatusChangedArg(sendingItem));
        return HandlerResult.Success(decision.Message);
    }

    private async Task ScheduleRetryAsync(
        SendingContext context,
        SendItemExecution currentAttempt,
        string message
    )
    {
        var retryCount = currentAttempt.Descriptor.TriedCount + 1;
        await context.SqlContext.SendingItems.UpdateAsync(
            x => x.Id == currentAttempt.Descriptor.Id,
            x =>
                x.SetProperty(y => y.Status, SendingItemStatus.Pending)
                    .SetProperty(y => y.TriedCount, retryCount)
                    .SetProperty(y => y.SendResult, message)
        );

        var baseSeconds = Math.Min(60, 1 << Math.Min(retryCount, 6));
        var jitterMilliseconds = RandomNumberGenerator.GetInt32(0, baseSeconds * 250 + 1);
        var retryAt =
            timeProvider.GetUtcNow()
            + TimeSpan.FromSeconds(baseSeconds)
            + TimeSpan.FromMilliseconds(jitterMilliseconds);
        if (context.GroupTask?.ScheduleRetryEmailItem(currentAttempt, retryAt) != true)
            throw new InvalidOperationException($"发件项 {currentAttempt.Descriptor.Id} 无法进入延迟重试队列");
    }

    private static async Task<SendingItem> SaveSendingItemAsync(
        SendingContext context,
        SendItemExecution currentAttempt,
        SendAttemptDecision decision
    )
    {
        var item = currentAttempt.PreparedItem;
        var db = context.SqlContext;
        var sendingItem = await db.SendingItems.FirstAsync(x =>
            x.Id == currentAttempt.Descriptor.Id
        );
        sendingItem.FromEmail = item.Outbox.Email;
        sendingItem.Subject = item.Subject;
        sendingItem.Content = item.HtmlBody;
        sendingItem.Status =
            decision.Disposition == SendAttemptDisposition.Succeeded
                ? SendingItemStatus.Success
                : SendingItemStatus.Failed;
        sendingItem.SendResult = decision.Message;
        sendingItem.TriedCount = currentAttempt.Descriptor.TriedCount;
        sendingItem.SendDate = DateTime.UtcNow;
        sendingItem.ReceiptId =
            decision.ReceiptId ?? new ResultParser(decision.Message).GetReceiptId();

        await db.SendingItemInboxes.UpdateAsync(
            x => x.SendingItemId == currentAttempt.Descriptor.Id,
            x =>
                x.SetProperty(y => y.FromEmail, item.Outbox.Email)
                    .SetProperty(y => y.SendDate, DateTime.UtcNow)
        );
        var inboxIds = item.Inboxes.Select(x => x.Id).ToList();
        await db.Inboxes.UpdateAsync(
            x => inboxIds.Contains(x.Id),
            x => x.SetProperty(y => y.LastBeDeliveredDate, DateTime.UtcNow)
        );
        if (decision.Disposition == SendAttemptDisposition.Succeeded)
        {
            await db.Inboxes.UpdateAsync(
                x => inboxIds.Contains(x.Id),
                x => x.SetProperty(y => y.LastSuccessDeliveryDate, DateTime.UtcNow)
            );
        }
        await db.SaveChangesAsync();
        return sendingItem;
    }
}
