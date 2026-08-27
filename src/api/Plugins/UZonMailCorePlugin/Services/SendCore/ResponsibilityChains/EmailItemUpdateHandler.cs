using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.EmailVerification;
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
public sealed class EmailItemUpdateHandler(
    TimeProvider timeProvider,
    RecipientContactFailureGroupService failureGroupService
) : AbstractSendingHandler
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
            context.SenderAccountRetirement
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

        var sendingItem = await SaveSendingItemAsync(
            context,
            currentAttempt,
            decision,
            transportResult
        );
        if (transportResult.FailureKind == SendFailureKind.HardBounce)
        {
            await MarkHardBounceRecipientContactAsync(context, currentAttempt, transportResult);
        }
        context.GroupTask?.CompleteEmailItem(currentAttempt);
        await context
            .HubClient.GetUserClient(currentAttempt.PreparedItem.SenderAccount.UserId)
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
        SendAttemptDecision decision,
        TransportResult transportResult
    )
    {
        var item = currentAttempt.PreparedItem;
        var db = context.SqlContext;
        var sendingItem = await db.SendingItems.FirstAsync(x =>
            x.Id == currentAttempt.Descriptor.Id
        );
        sendingItem.SenderEmail = item.SenderAccount.Email;
        sendingItem.Subject = item.Subject;
        sendingItem.Content = item.HtmlBody;
        sendingItem.Status =
            decision.Disposition == SendAttemptDisposition.Succeeded
                ? SendingItemStatus.Success
                : SendingItemStatus.Failed;
        sendingItem.SendResult = decision.Message;
        sendingItem.TriedCount = currentAttempt.Descriptor.TriedCount;
        sendingItem.IsHardBounce = transportResult.FailureKind == SendFailureKind.HardBounce;
        sendingItem.SendDate = DateTime.UtcNow;
        sendingItem.ReceiptId =
            decision.ReceiptId ?? new ResultParser(decision.Message).GetReceiptId();

        await db.SendingItemRecipients.UpdateAsync(
            x => x.SendingItemId == currentAttempt.Descriptor.Id,
            x =>
                x.SetProperty(y => y.SenderEmail, item.SenderAccount.Email)
                    .SetProperty(y => y.SendDate, DateTime.UtcNow)
        );
        var recipientContactIds = item.Recipients.Select(x => x.Id).ToList();
        await db.RecipientContacts.UpdateAsync(
            x => recipientContactIds.Contains(x.Id),
            x => x.SetProperty(y => y.LastDeliveredAtUtc, DateTime.UtcNow)
        );
        if (decision.Disposition == SendAttemptDisposition.Succeeded)
        {
            await db.RecipientContacts.UpdateAsync(
                x => recipientContactIds.Contains(x.Id),
                x => x.SetProperty(y => y.LastSuccessDeliveryDate, DateTime.UtcNow)
            );
        }
        await db.SaveChangesAsync();
        return sendingItem;
    }

    private async Task MarkHardBounceRecipientContactAsync(
        SendingContext context,
        SendItemExecution currentAttempt,
        TransportResult transportResult
    )
    {
        var db = context.SqlContext;
        var sendingItemId = currentAttempt.Descriptor.Id;
        var recipientContactLinks = await db
            .SendingItemRecipients.Where(x => x.SendingItemId == sendingItemId)
            .ToListAsync();
        if (recipientContactLinks.Count == 0)
            return;

        SendingItemRecipient? targetLink = null;
        if (!string.IsNullOrWhiteSpace(transportResult.RejectedRecipientEmail))
        {
            targetLink = recipientContactLinks.FirstOrDefault(x =>
                string.Equals(
                    x.RecipientEmail,
                    transportResult.RejectedRecipientEmail,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
        else if (recipientContactLinks.Count == 1)
        {
            // SMTP 未返回被拒地址时，多个收件人无法安全归因，不能误迁移。
            targetLink = recipientContactLinks[0];
        }

        if (targetLink is null || targetLink.RecipientContactId <= 0)
            return;

        var recipientContact = await db.RecipientContacts.FirstOrDefaultAsync(x =>
            x.Id == targetLink.RecipientContactId && x.UserId == currentAttempt.PreparedItem.UserId
        );
        if (recipientContact is null)
            return;

        var reason = transportResult.Message;
        await failureGroupService.MarkInvalidAsync(
            recipientContact.UserId,
            new Dictionary<long, string?> { [recipientContact.Id] = reason }
        );
    }
}
