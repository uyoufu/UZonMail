using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.MailConversations;

namespace UzonMail.CorePlugin.Controllers.MailConversations.DTOs;

public sealed record MailTagDto(long Id, string Name, string Color);

public sealed record MailContactDto(
    long Id,
    string Email,
    string? DisplayName,
    IReadOnlyList<MailTagDto> Tags
);

public sealed record MailConversationListItemDto(
    long Id,
    long EmailAccountId,
    string EmailAccount,
    MailConversationType ConversationType,
    string DisplayTitle,
    DateTime LastMessageAtUtc,
    string? LastMessagePreview,
    int UnreadCount,
    IReadOnlyList<MailContactDto> Participants
);

public sealed record MailAddressDto(string Email, string? DisplayName);

public sealed record MailAttachmentDto(
    long Id,
    string FileName,
    string ContentType,
    long? Size,
    bool IsOutgoingFileUsage
);

public sealed record MailConversationMessageDto(
    long Id,
    MailMessageDirection Direction,
    string? Subject,
    DateTime OccurredAtUtc,
    bool IsRead,
    SendingItemStatus? SendingStatus,
    IReadOnlyList<MailAddressDto> From,
    IReadOnlyList<MailAddressDto> To,
    IReadOnlyList<MailAddressDto> Cc,
    string? PreviewText,
    IReadOnlyList<MailAttachmentDto> Attachments
);

public sealed record MailMessageContentDto(
    long ConversationMessageId,
    string? HtmlBody,
    string? TextBody,
    IReadOnlyList<MailAttachmentDto> Attachments
);

public enum MailReplyMode
{
    Reply = 0,
    ReplyAll = 1,
}

public sealed class SendConversationMessageRequest
{
    public long? ReplyToMessageId { get; set; }
    public MailReplyMode ReplyMode { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public List<long> AttachmentFileUsageIds { get; set; } = [];
}

public sealed record SendConversationMessageResult(long ConversationMessageId, long SendingItemId);

public sealed class UpsertMailTagRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#1976d2";
}

public sealed class SetMailContactTagsRequest
{
    public List<long> TagIds { get; set; } = [];
}
