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
}

public sealed record MailboxAttachmentContent(
    string FileName,
    string ContentType,
    MemoryStream Content
);
