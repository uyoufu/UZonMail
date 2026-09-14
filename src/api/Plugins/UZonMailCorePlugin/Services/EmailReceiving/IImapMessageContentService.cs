namespace UzonMail.CorePlugin.Services.EmailReceiving;

public interface IImapMessageContentService
{
    Task EnsureBodyAsync(
        long userId,
        long mailboxMessageId,
        CancellationToken cancellationToken = default
    );

    Task<MailboxAttachmentContent> GetAttachmentAsync(
        long userId,
        long mailboxMessageId,
        long mimePartId,
        CancellationToken cancellationToken = default
    );

    Task<MailboxMessageHeaderMetadata?> GetMetadataAsync(
        long userId,
        long mailboxMessageId,
        CancellationToken cancellationToken = default
    );
}

public sealed record MailboxAttachmentContent(
    string FileName,
    string ContentType,
    MemoryStream Content
);

public sealed record MailboxMessageHeaderMetadata(
    DateTimeOffset? DeclaredSentAt,
    IReadOnlyList<string> ReceivedHeaders
);
