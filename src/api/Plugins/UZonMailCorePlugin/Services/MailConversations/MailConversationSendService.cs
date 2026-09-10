using Microsoft.EntityFrameworkCore;
using MimeKit.Utils;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.DB.SQL.Core.Todos;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 将会话回复转换为 SendCore 发件任务，并在入队前固化 RFC 线程头。
/// </summary>
public sealed class MailConversationSendService(
    SqlContext db,
    ISendingGroupCreationService creationService,
    ISendingGroupCommandService commandService
) : IScopedService
{
    public Task<SendConversationMessageResult> SendAsync(
        long userId,
        long conversationId,
        SendConversationMessageRequest request,
        CancellationToken cancellationToken = default
    ) => SendCoreAsync(userId, conversationId, null, request, cancellationToken);

    public async Task<SendConversationMessageResult> SendTodoBranchAsync(
        long userId,
        long todoTaskId,
        SendConversationMessageRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var branch =
            await db
                .TodoMailBranches.Include(x => x.TodoTask)
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(
                    x => x.TodoTaskId == todoTaskId && x.TodoTask.UserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("邮件待办不存在");
        return await SendCoreAsync(
            userId,
            branch.SourceConversationId,
            branch,
            request,
            cancellationToken
        );
    }

    private async Task<SendConversationMessageResult> SendCoreAsync(
        long userId,
        long conversationId,
        TodoMailBranch? todoBranch,
        SendConversationMessageRequest request,
        CancellationToken cancellationToken
    )
    {
        ValidateRequest(request);
        var conversation =
            await db
                .MailConversations.Include(x => x.EmailAccount)
                .ThenInclude(x => x.SenderAccount)
                .Include(x => x.Participants)
                .ThenInclude(x => x.MailContact)
                .FirstOrDefaultAsync(
                    x => x.Id == conversationId && x.UserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("邮件会话不存在");
        var senderAccount = conversation.EmailAccount.SenderAccount;
        if (
            senderAccount is null
            || senderAccount.Status != UzonMail.DB.SQL.Core.Emails.SenderAccountStatus.Valid
        )
            throw new KnownException("当前收件账号没有可用的发件配置");

        var replyTarget = await ResolveReplyTargetAsync(
            conversationId,
            todoBranch,
            request.ReplyToMessageId,
            cancellationToken
        );
        var recipients = ResolveRecipients(conversation, replyTarget, request.ReplyMode);
        if (recipients.Count == 0)
            throw new KnownException("会话中没有可回复的外部联系人");
        var parentHeaders = ResolveParentHeaders(
            replyTarget,
            todoBranch is not null && todoBranch.RootSendingItemId is null
        );
        var group = new SendingGroup
        {
            SourceType = todoBranch is null
                ? SendingGroupSourceType.ConversationReply
                : SendingGroupSourceType.TodoFollowUp,
            Subjects = request.Subject.Trim(),
            Body = request.HtmlBody,
            SenderAccounts = [senderAccount],
            Recipients = recipients,
            Attachments = request
                .AttachmentFileUsageIds.Select(x => new UzonMail.DB.SQL.Core.Files.FileUsage
                {
                    __fileUsageId = x
                })
                .ToList(),
            SendBatch = true,
            ScheduleDate = DateTime.UtcNow,
            SendingType = SendingGroupType.Instant,
        };
        var persistedGroup = await creationService.CreateSendingGroup(group);
        var sendingItem = await db
            .SendingItems.Include(x => x.Attachments)
            .SingleAsync(x => x.SendingGroupId == persistedGroup.Id, cancellationToken);
        var internetMessageId = MimeUtils.GenerateMessageId();
        sendingItem.InternetMessageId = internetMessageId;
        sendingItem.InternetMessageIdKey = internetMessageId.Trim().ToLowerInvariant();
        sendingItem.InReplyToInternetMessageId = parentHeaders.InReplyTo;
        sendingItem.ReferenceInternetMessageIds = parentHeaders.References;
        sendingItem.Subject = request.Subject.Trim();
        sendingItem.Content = request.HtmlBody;

        var timelineMessage = new MailConversationMessage
        {
            MailConversationId = conversation.Id,
            SendingItemId = sendingItem.Id,
            SourceKey = $"sending:{sendingItem.Id}",
            Direction = MailMessageDirection.Outgoing,
            OccurredAtUtc = DateTime.UtcNow,
            IsRead = true,
            ReplyToConversationMessageId = replyTarget?.Id,
        };
        db.MailConversationMessages.Add(timelineMessage);
        conversation.LastMessageAtUtc = timelineMessage.OccurredAtUtc;
        conversation.LastMessagePreview = MailMessagePreviewFormatter.Normalize(request.HtmlBody);
        if (todoBranch is not null)
        {
            db.TodoMailBranchMessages.Add(
                new TodoMailBranchMessage
                {
                    TodoMailBranchId = todoBranch.Id,
                    MailConversationMessage = timelineMessage
                }
            );
            todoBranch.RootSendingItemId ??= sendingItem.Id;
        }
        await db.SaveChangesAsync(cancellationToken);
        await commandService.SendNow(persistedGroup, [sendingItem.Id]);
        return new SendConversationMessageResult(timelineMessage.Id, sendingItem.Id);
    }

    private async Task<MailConversationMessage?> ResolveReplyTargetAsync(
        long conversationId,
        TodoMailBranch? todoBranch,
        long? requestedMessageId,
        CancellationToken cancellationToken
    )
    {
        if (todoBranch is not null && todoBranch.RootSendingItemId is null)
            return null;
        if (requestedMessageId is > 0)
            return await LoadTimelineMessageAsync(
                conversationId,
                requestedMessageId.Value,
                cancellationToken
            );
        if (todoBranch is not null)
        {
            var branchMessageId = await db
                .TodoMailBranchMessages.Where(x => x.TodoMailBranchId == todoBranch.Id)
                .OrderByDescending(x => x.MailConversationMessage.OccurredAtUtc)
                .Select(x => (long?)x.MailConversationMessageId)
                .FirstOrDefaultAsync(cancellationToken);
            if (branchMessageId is > 0)
                return await LoadTimelineMessageAsync(
                    conversationId,
                    branchMessageId.Value,
                    cancellationToken
                );
        }
        return await db
            .MailConversationMessages.Where(x => x.MailConversationId == conversationId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.References)
            .Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.Addresses)
            .Include(x => x.SendingItem)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<MailConversationMessage> LoadTimelineMessageAsync(
        long conversationId,
        long messageId,
        CancellationToken cancellationToken
    ) =>
        await db
            .MailConversationMessages.Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.References)
            .Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.Addresses)
            .Include(x => x.SendingItem)
            .FirstOrDefaultAsync(
                x => x.Id == messageId && x.MailConversationId == conversationId,
                cancellationToken
            ) ?? throw new KnownException("回复的邮件不存在");

    private static List<UzonMail.DB.SQL.Core.EmailSending.EmailAddress> ResolveRecipients(
        MailConversation conversation,
        MailConversationMessage? replyTarget,
        MailReplyMode replyMode
    )
    {
        if (replyMode == MailReplyMode.Reply && replyTarget?.IncomingMailMessage is not null)
        {
            var replyAddress =
                replyTarget
                    .IncomingMailMessage.Addresses.Where(x =>
                        x.AddressType == IncomingMailAddressType.ReplyTo
                    )
                    .OrderBy(x => x.Position)
                    .FirstOrDefault()
                ?? replyTarget
                    .IncomingMailMessage.Addresses.Where(x =>
                        x.AddressType == IncomingMailAddressType.From
                    )
                    .OrderBy(x => x.Position)
                    .FirstOrDefault();
            if (replyAddress is not null)
                return
                [
                    new UzonMail.DB.SQL.Core.EmailSending.EmailAddress
                    {
                        Email = replyAddress.Email,
                        Name = replyAddress.DisplayName
                    }
                ];
        }
        return conversation
            .Participants.Where(x => x.IsActive)
            .Select(x => new UzonMail.DB.SQL.Core.EmailSending.EmailAddress
            {
                Email = x.MailContact.Email,
                Name = x.MailContact.DisplayName
            })
            .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();
    }

    private static ParentHeaders ResolveParentHeaders(
        MailConversationMessage? replyTarget,
        bool isNewTodoRoot
    )
    {
        if (replyTarget is null || isNewTodoRoot)
            return new ParentHeaders(null, []);
        if (replyTarget.SendingItem is not null)
        {
            var parentId = replyTarget.SendingItem.InternetMessageId;
            var references = replyTarget.SendingItem.ReferenceInternetMessageIds.ToList();
            AddDistinct(references, parentId);
            return new ParentHeaders(parentId, references);
        }
        var incoming = replyTarget.IncomingMailMessage;
        if (incoming is null)
            return new ParentHeaders(null, []);
        var incomingReferences = incoming
            .References.Where(x => x.ReferenceType == IncomingMailReferenceType.References)
            .OrderBy(x => x.Position)
            .Select(x => x.InternetMessageId)
            .ToList();
        AddDistinct(incomingReferences, incoming.InternetMessageId);
        return new ParentHeaders(incoming.InternetMessageId, incomingReferences);
    }

    private static void AddDistinct(List<string> references, string? messageId)
    {
        if (
            !string.IsNullOrWhiteSpace(messageId)
            && !references.Contains(messageId, StringComparer.OrdinalIgnoreCase)
        )
            references.Add(messageId);
    }

    private static void ValidateRequest(SendConversationMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new KnownException("邮件主题不能为空");
        if (string.IsNullOrWhiteSpace(request.HtmlBody))
            throw new KnownException("邮件正文不能为空");
    }

    private sealed record ParentHeaders(string? InReplyTo, List<string> References);
}
