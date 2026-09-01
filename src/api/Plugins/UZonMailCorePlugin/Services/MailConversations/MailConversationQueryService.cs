using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Services.EmailReceiving;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 为会话列表和时间线提供只读、游标分页的查询边界。
/// </summary>
public sealed class MailConversationQueryService(
    SqlContext db,
    IImapMessageContentService messageContentService
) : IScopedService
{
    public async Task<List<MailConversationListItemDto>> GetConversationsAsync(
        long userId,
        long? emailAccountId,
        long? tagId,
        bool unreadOnly,
        string? filter,
        DateTime? beforeAtUtc,
        long? beforeId,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        limit = Math.Clamp(limit, 1, 100);
        var query = db.MailConversations.AsNoTracking().Where(x => x.UserId == userId);
        if (emailAccountId is > 0)
            query = query.Where(x => x.EmailAccountId == emailAccountId);
        if (tagId is > 0)
            query = query.Where(x =>
                x.Participants.Any(p => p.MailContact.Tags.Any(t => t.MailTagId == tagId))
            );
        if (unreadOnly)
            query = query.Where(x => x.UnreadCount > 0);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var normalizedFilter = filter.Trim();
            query = query.Where(x =>
                (x.DisplayTitle != null && x.DisplayTitle.Contains(normalizedFilter))
                || x.Participants.Any(p =>
                    p.MailContact.Email.Contains(normalizedFilter)
                    || (
                        p.MailContact.DisplayName != null
                        && p.MailContact.DisplayName.Contains(normalizedFilter)
                    )
                )
            );
        }
        if (beforeAtUtc is not null)
        {
            var cursorId = beforeId ?? long.MaxValue;
            query = query.Where(x =>
                x.LastMessageAtUtc < beforeAtUtc
                || (x.LastMessageAtUtc == beforeAtUtc && x.Id < cursorId)
            );
        }

        var conversations = await query
            .OrderByDescending(x => x.LastMessageAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .Include(x => x.EmailAccount)
            .Include(x => x.Participants)
            .ThenInclude(x => x.MailContact)
            .ThenInclude(x => x.Tags)
            .ThenInclude(x => x.MailTag)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return conversations.Select(ToListItemDto).ToList();
    }

    public async Task<List<MailConversationMessageDto>> GetMessagesAsync(
        long userId,
        long conversationId,
        DateTime? beforeAtUtc,
        long? beforeId,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureConversationOwnedAsync(userId, conversationId, cancellationToken);
        limit = Math.Clamp(limit, 1, 100);
        var query = db
            .MailConversationMessages.AsNoTracking()
            .Where(x => x.MailConversationId == conversationId);
        if (beforeAtUtc is not null)
        {
            var cursorId = beforeId ?? long.MaxValue;
            query = query.Where(x =>
                x.OccurredAtUtc < beforeAtUtc || (x.OccurredAtUtc == beforeAtUtc && x.Id < cursorId)
            );
        }
        var messages = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.Addresses)
            .Include(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.MimeParts)
            .Include(x => x.SendingItem)
            .ThenInclude(x => x!.Attachments!)
            .ThenInclude(x => x.FileObject)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        messages.Reverse();
        return messages.Select(ToMessageDto).ToList();
    }

    public async Task<MailMessageContentDto> GetMessageContentAsync(
        long userId,
        long conversationMessageId,
        CancellationToken cancellationToken = default
    )
    {
        var timelineMessage =
            await db
                .MailConversationMessages.Include(x => x.MailConversation)
                .Include(x => x.IncomingMailMessage)
                .ThenInclude(x => x!.MimeParts)
                .Include(x => x.SendingItem)
                .ThenInclude(x => x!.Attachments!)
                .ThenInclude(x => x.FileObject)
                .FirstOrDefaultAsync(
                    x => x.Id == conversationMessageId && x.MailConversation.UserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("会话邮件不存在");

        if (timelineMessage.IncomingMailMessageId is > 0)
        {
            await messageContentService.EnsureBodyAsync(
                userId,
                timelineMessage.IncomingMailMessageId.Value,
                cancellationToken
            );
            await db.Entry(timelineMessage.IncomingMailMessage!).ReloadAsync(cancellationToken);
            await db.Entry(timelineMessage.IncomingMailMessage!)
                .Collection(x => x.MimeParts)
                .LoadAsync(cancellationToken);
        }
        if (!timelineMessage.IsRead)
        {
            timelineMessage.IsRead = true;
            if (timelineMessage.MailConversation.UnreadCount > 0)
                timelineMessage.MailConversation.UnreadCount--;
            timelineMessage.MailConversation.LastReadAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        var attachments = CreateAttachments(timelineMessage);
        return new MailMessageContentDto(
            timelineMessage.Id,
            timelineMessage.IncomingMailMessage?.CachedHtmlBody
                ?? timelineMessage.SendingItem?.Content,
            timelineMessage.IncomingMailMessage?.CachedTextBody,
            attachments
        );
    }

    public async Task MarkConversationReadAsync(
        long userId,
        long conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var conversation =
            await db.MailConversations.FirstOrDefaultAsync(
                x => x.Id == conversationId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮件会话不存在");
        await db
            .MailConversationMessages.Where(x =>
                x.MailConversationId == conversationId && !x.IsRead
            )
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRead, true), cancellationToken);
        conversation.UnreadCount = 0;
        conversation.LastReadAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<long> GetIncomingMessageIdAsync(
        long userId,
        long conversationMessageId,
        CancellationToken cancellationToken = default
    ) =>
        await db
            .MailConversationMessages.Where(x =>
                x.Id == conversationMessageId && x.MailConversation.UserId == userId
            )
            .Select(x => x.IncomingMailMessageId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new KnownException("入站邮件不存在");

    private async Task EnsureConversationOwnedAsync(
        long userId,
        long conversationId,
        CancellationToken cancellationToken
    )
    {
        var isOwned = await db.MailConversations.AnyAsync(
            x => x.Id == conversationId && x.UserId == userId,
            cancellationToken
        );
        if (!isOwned)
            throw new KnownException("邮件会话不存在");
    }

    private static MailConversationListItemDto ToListItemDto(MailConversation conversation) =>
        new(
            conversation.Id,
            conversation.EmailAccountId,
            conversation.EmailAccount.Email,
            conversation.ConversationType,
            conversation.DisplayTitle ?? "",
            conversation.LastMessageAtUtc,
            conversation.LastMessagePreview,
            conversation.UnreadCount,
            conversation
                .Participants.Where(x => x.IsActive)
                .Select(x => new MailContactDto(
                    x.MailContact.Id,
                    x.MailContact.Email,
                    x.MailContact.DisplayName,
                    x.MailContact.Tags.Select(t => new MailTagDto(
                        t.MailTag.Id,
                        t.MailTag.Name,
                        t.MailTag.Color
                    ))
                        .ToList()
                ))
                .ToList()
        );

    internal static MailConversationMessageDto ToMessageDto(MailConversationMessage message)
    {
        var incoming = message.IncomingMailMessage;
        var sending = message.SendingItem;
        return new MailConversationMessageDto(
            message.Id,
            message.Direction,
            incoming?.Subject ?? sending?.Subject,
            message.OccurredAtUtc,
            message.IsRead,
            sending?.Status,
            incoming is null
                ? CreateAddressList(sending?.SenderEmail)
                : CreateAddressList(incoming.Addresses, IncomingMailAddressType.From),
            incoming is null
                ? CreateAddressList(sending?.Recipients)
                : CreateAddressList(incoming.Addresses, IncomingMailAddressType.To),
            incoming is null
                ? CreateAddressList(sending?.CC)
                : CreateAddressList(incoming.Addresses, IncomingMailAddressType.Cc),
            CreateAttachments(message)
        );
    }

    private static List<MailAttachmentDto> CreateAttachments(MailConversationMessage message)
    {
        if (message.IncomingMailMessage is not null)
        {
            return message
                .IncomingMailMessage.MimeParts.Where(x =>
                    x.PartKind == IncomingMailMimePartKind.Attachment
                )
                .Select(x => new MailAttachmentDto(
                    x.Id,
                    x.FileName ?? "attachment",
                    x.MediaType ?? "application/octet-stream",
                    x.DeclaredSize,
                    false
                ))
                .ToList();
        }
        return message
                .SendingItem?.Attachments?.Select(x => new MailAttachmentDto(
                    x.Id,
                    x.DisplayName,
                    "application/octet-stream",
                    x.FileObject.Size,
                    true
                ))
                .ToList() ?? [];
    }

    private static List<MailAddressDto> CreateAddressList(
        IEnumerable<IncomingMailAddress> addresses,
        IncomingMailAddressType addressType
    ) =>
        addresses
            .Where(x => x.AddressType == addressType)
            .OrderBy(x => x.Position)
            .Select(x => new MailAddressDto(x.Email, x.DisplayName))
            .ToList();

    private static List<MailAddressDto> CreateAddressList(IEnumerable<EmailAddress>? addresses) =>
        addresses?.Select(x => new MailAddressDto(x.Email, x.Name)).ToList() ?? [];

    private static List<MailAddressDto> CreateAddressList(string? address) =>
        string.IsNullOrWhiteSpace(address) ? [] : [new MailAddressDto(address, null)];
}
