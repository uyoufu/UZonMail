using MailKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

/// <summary>
/// 在用户实际查看时下载正文或附件，避免初次同步被大邮件阻塞。
/// </summary>
public sealed class ImapMessageContentService(
    SqlContext db,
    ICredentialProtector credentialProtector
) : IImapMessageContentService, IScopedService<IImapMessageContentService>
{
    public async Task EnsureBodyAsync(
        long userId,
        long mailboxMessageId,
        CancellationToken cancellationToken = default
    )
    {
        var mailboxMessage = await GetOwnedMessageAsync(
            userId,
            mailboxMessageId,
            cancellationToken
        );
        if (
            mailboxMessage.BodyContentStatus == IncomingMailBodyContentStatus.Available
            && mailboxMessage.BodyExpiresAtUtc > DateTime.UtcNow
        )
            return;

        mailboxMessage.BodyContentStatus = IncomingMailBodyContentStatus.Downloading;
        await db.SaveChangesAsync(cancellationToken);
        try
        {
            using var connection = await OpenMessageAsync(mailboxMessage, true, cancellationToken);
            var mimeMessage = await connection.Folder.GetMessageAsync(
                connection.UniqueId,
                cancellationToken
            );
            mailboxMessage.CachedHtmlBody = mimeMessage.HtmlBody;
            mailboxMessage.CachedTextBody = mimeMessage.TextBody;
            mailboxMessage.BodyCachedAtUtc = DateTime.UtcNow;
            mailboxMessage.BodyExpiresAtUtc = DateTime.UtcNow.AddDays(
                mailboxMessage.ReceivingAccount.ContentRetentionDays
            );
            mailboxMessage.BodyContentStatus = IncomingMailBodyContentStatus.Available;
            await ReplaceAttachmentMetadataAsync(mailboxMessage, mimeMessage, cancellationToken);
            await MarkSeenAsync(mailboxMessage, connection, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            mailboxMessage.BodyContentStatus = IncomingMailBodyContentStatus.Failed;
            var bodyPart = mailboxMessage.MimeParts.FirstOrDefault(x =>
                x.PartKind
                    is IncomingMailMimePartKind.HtmlBody
                        or IncomingMailMimePartKind.PlainTextBody
            );
            if (bodyPart is not null)
                bodyPart.LastFetchError = exception.Message;
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<MailboxAttachmentContent> GetAttachmentAsync(
        long userId,
        long mailboxMessageId,
        long mimePartId,
        CancellationToken cancellationToken = default
    )
    {
        var mailboxMessage = await GetOwnedMessageAsync(
            userId,
            mailboxMessageId,
            cancellationToken
        );
        var mimePart =
            mailboxMessage.MimeParts.FirstOrDefault(x =>
                x.Id == mimePartId && x.PartKind == IncomingMailMimePartKind.Attachment
            ) ?? throw new KnownException("邮件附件不存在");
        if (!TryParseAttachmentIndex(mimePart.MimePartPath, out var attachmentIndex))
            throw new KnownException("邮件附件索引无效");

        using var connection = await OpenMessageAsync(mailboxMessage, false, cancellationToken);
        var mimeMessage = await connection.Folder.GetMessageAsync(
            connection.UniqueId,
            cancellationToken
        );
        var attachment =
            mimeMessage.Attachments.ElementAtOrDefault(attachmentIndex)
            ?? throw new KnownException("远端邮件附件不存在");
        var stream = new MemoryStream();
        if (attachment is MessagePart messagePart && messagePart.Message is not null)
            await messagePart.Message.WriteToAsync(stream, cancellationToken);
        else if (attachment is MimePart binaryPart)
        {
            if (binaryPart.Content is null)
                throw new KnownException("邮件附件内容为空");
            await binaryPart.Content.DecodeToAsync(stream, cancellationToken);
        }
        else
            throw new KnownException("不支持的邮件附件类型");
        stream.Position = 0;
        var fileName = mimePart.FileName ?? $"attachment-{attachmentIndex + 1}";
        var contentType = mimePart.MediaType ?? "application/octet-stream";
        return new MailboxAttachmentContent(fileName, contentType, stream);
    }

    /// <summary>
    /// 读取邮件原始头中的展示元数据，不缓存正文或改变邮件已读状态。
    /// </summary>
    public async Task<MailboxMessageHeaderMetadata?> GetMetadataAsync(
        long userId,
        long mailboxMessageId,
        CancellationToken cancellationToken = default
    )
    {
        var mailboxMessage = await GetOwnedMessageAsync(
            userId,
            mailboxMessageId,
            cancellationToken
        );
        if (!mailboxMessage.Locations.Any(x => x.IsPresentOnServer))
            return null;

        using var connection = await OpenMessageAsync(mailboxMessage, false, cancellationToken);
        var summary = (
            await connection.Folder.FetchAsync(
                [connection.UniqueId],
                MessageSummaryItems.Envelope | MessageSummaryItems.Headers,
                cancellationToken
            )
        ).FirstOrDefault();
        if (summary is null)
            return null;
        var declaredSentAt = summary.Envelope?.Date;
        var receivedHeaders = summary.Headers?
            .Where(x => x.Field.Equals("Received", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value)
            .ToList() ?? [];
        return new MailboxMessageHeaderMetadata(declaredSentAt, receivedHeaders);
    }

    private async Task<IncomingMailMessage> GetOwnedMessageAsync(
        long userId,
        long mailboxMessageId,
        CancellationToken cancellationToken
    ) =>
        await db
            .IncomingMailMessages.Include(x => x.ReceivingAccount)
            .ThenInclude(x => x.EmailAccount)
            .Include(x => x.Locations)
            .ThenInclude(x => x.ImapMailbox)
            .Include(x => x.MimeParts)
            .FirstOrDefaultAsync(
                x => x.Id == mailboxMessageId && x.ReceivingAccount.EmailAccount.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮件不存在");

    private async Task<OpenedImapMessage> OpenMessageAsync(
        IncomingMailMessage mailboxMessage,
        bool writeAccess,
        CancellationToken cancellationToken
    )
    {
        var location =
            mailboxMessage
                .Locations.Where(x => x.IsPresentOnServer)
                .OrderByDescending(x => x.LastSynchronizedAtUtc)
                .FirstOrDefault() ?? throw new KnownException("邮件已不在远端邮箱中");
        var credential =
            await db.ReceivingAccountImapCredentials.FirstOrDefaultAsync(
                x => x.ReceivingAccountId == mailboxMessage.ReceivingAccountId,
                cancellationToken
            ) ?? throw new KnownException("IMAP 凭据未配置");
        if (
            string.IsNullOrWhiteSpace(credential.EncryptedPassword)
            || string.IsNullOrWhiteSpace(credential.EncryptionKeyVersion)
        )
            throw new KnownException("IMAP 凭据不完整");

        var client = new ImapClient();
        await client.ConnectAsync(
            credential.Host,
            credential.Port,
            credential.ConnectionSecurity.ToMailKitSecureSocketOptions(),
            cancellationToken
        );
        var password = credentialProtector.Unprotect(
            credential.EncryptedPassword,
            credential.EncryptionKeyVersion
        );
        await client.AuthenticateAsync(credential.LoginName, password, cancellationToken);
        await ImapClientIdentification.IdentifyAsync(client, cancellationToken);
        var folder = await client.GetFolderAsync(
            location.ImapMailbox.RemoteFullName,
            cancellationToken
        );
        await folder.OpenAsync(
            writeAccess ? FolderAccess.ReadWrite : FolderAccess.ReadOnly,
            cancellationToken
        );
        return new OpenedImapMessage(client, folder, new UniqueId((uint)location.Uid), location);
    }

    private async Task ReplaceAttachmentMetadataAsync(
        IncomingMailMessage mailboxMessage,
        MimeMessage mimeMessage,
        CancellationToken cancellationToken
    )
    {
        var existingParts = mailboxMessage.MimeParts.ToDictionary(x => x.MimePartPath);
        var retainedPaths = new HashSet<string>(StringComparer.Ordinal);
        var attachmentIndex = 0;
        foreach (var attachment in mimeMessage.Attachments)
        {
            var mimePartPath = $"attachment:{attachmentIndex++}";
            var contentType = attachment.ContentType;
            var fileName = attachment.ContentDisposition?.FileName ?? contentType.Name;
            retainedPaths.Add(mimePartPath);
            if (!existingParts.TryGetValue(mimePartPath, out var mimePart))
            {
                mimePart = new IncomingMailMimePart
                {
                    IncomingMailMessageId = mailboxMessage.Id,
                    MimePartPath = mimePartPath,
                };
                db.IncomingMailMimeParts.Add(mimePart);
            }
            mimePart.IsDeleted = false;
            mimePart.PartKind = IncomingMailMimePartKind.Attachment;
            mimePart.ContentDisposition = IncomingMailContentDisposition.Attachment;
            mimePart.FileName = fileName;
            mimePart.MediaType = contentType.MimeType;
            mimePart.FetchStatus = IncomingMailMimePartFetchStatus.NotDownloaded;
            mimePart.ExpiresAtUtc = mailboxMessage.BodyExpiresAtUtc;
        }
        foreach (var existingPart in mailboxMessage.MimeParts)
            existingPart.IsDeleted = !retainedPaths.Contains(existingPart.MimePartPath);
        mailboxMessage.AttachmentCount = attachmentIndex;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task MarkSeenAsync(
        IncomingMailMessage mailboxMessage,
        OpenedImapMessage connection,
        CancellationToken cancellationToken
    )
    {
        if (mailboxMessage.Direction != MailMessageDirection.Incoming)
            return;
        await connection.Folder.AddFlagsAsync(
            connection.UniqueId,
            MessageFlags.Seen,
            true,
            cancellationToken
        );
        connection.Location.Flags |= ImapMessageFlags.Seen;
    }

    private static bool TryParseAttachmentIndex(string mimePartPath, out int attachmentIndex)
    {
        const string prefix = "attachment:";
        attachmentIndex = -1;
        return mimePartPath.StartsWith(prefix, StringComparison.Ordinal)
            && int.TryParse(mimePartPath[prefix.Length..], out attachmentIndex)
            && attachmentIndex >= 0;
    }

    private sealed class OpenedImapMessage(
        ImapClient client,
        IMailFolder folder,
        UniqueId uniqueId,
        IncomingMailLocation location
    ) : IDisposable
    {
        public IMailFolder Folder { get; } = folder;
        public UniqueId UniqueId { get; } = uniqueId;
        public IncomingMailLocation Location { get; } = location;

        public void Dispose()
        {
            if (Folder.IsOpen)
                Folder.Close();
            if (client.IsConnected)
                client.Disconnect(true);
            client.Dispose();
        }
    }
}
