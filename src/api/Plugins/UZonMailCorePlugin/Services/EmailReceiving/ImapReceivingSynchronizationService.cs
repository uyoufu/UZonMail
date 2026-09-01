using System.Collections.Concurrent;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

/// <summary>
/// 执行可重入的 IMAP 增量同步；长连接协调器和手动同步共同复用该边界。
/// </summary>
public sealed class ImapReceivingSynchronizationService(
    SqlContext db,
    ICredentialProtector credentialProtector,
    IMailConversationIngestionService conversationIngestionService
) : IReceivingSynchronizationService, IScopedService<IReceivingSynchronizationService>
{
    private const int InitialLookbackDays = 90;
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> AccountLocks = [];

    public async Task<ReceivingSynchronizationResult> SynchronizeAsync(
        long userId,
        long receivingAccountId,
        ImapSyncTrigger trigger,
        CancellationToken cancellationToken = default
    )
    {
        var accountLock = AccountLocks.GetOrAdd(
            receivingAccountId,
            static _ => new SemaphoreSlim(1, 1)
        );
        await accountLock.WaitAsync(cancellationToken);
        try
        {
            return await SynchronizeAccountAsync(
                userId,
                receivingAccountId,
                trigger,
                cancellationToken
            );
        }
        finally
        {
            accountLock.Release();
        }
    }

    private async Task<ReceivingSynchronizationResult> SynchronizeAccountAsync(
        long userId,
        long receivingAccountId,
        ImapSyncTrigger trigger,
        CancellationToken cancellationToken
    )
    {
        var receivingAccount =
            await db
                .ReceivingAccounts.Include(x => x.EmailAccount)
                .FirstOrDefaultAsync(
                    x => x.Id == receivingAccountId && x.EmailAccount.UserId == userId,
                    cancellationToken
                ) ?? throw new KnownException("收件账户不存在");
        if (receivingAccount.Protocol != ReceivingProtocol.Imap)
            throw new KnownException("首版收件同步仅支持 IMAP");
        if (receivingAccount.Status == ReceivingAccountStatus.Paused)
            throw new KnownException("收件账户已暂停");

        var credential =
            await db.ReceivingAccountImapCredentials.FirstOrDefaultAsync(
                x => x.ReceivingAccountId == receivingAccountId,
                cancellationToken
            ) ?? throw new KnownException("IMAP 凭据未配置");
        if (
            string.IsNullOrWhiteSpace(credential.EncryptedPassword)
            || string.IsNullOrWhiteSpace(credential.EncryptionKeyVersion)
        )
            throw new KnownException("IMAP 凭据不完整");

        var syncRun = new ImapSyncRun
        {
            ReceivingAccountId = receivingAccountId,
            Trigger = trigger,
            Status = ImapSyncStatus.Running,
            StartedAtUtc = DateTime.UtcNow,
        };
        db.ImapSyncRuns.Add(syncRun);
        receivingAccount.LastSyncAttemptAtUtc = syncRun.StartedAtUtc;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            using var client = new ImapClient();
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

            var folders = GetSynchronizationFolders(client);
            syncRun.MailboxesAttempted = folders.Count;
            foreach (var folderDefinition in folders)
            {
                await SynchronizeFolderAsync(
                    receivingAccount,
                    syncRun,
                    folderDefinition.Folder,
                    folderDefinition.SpecialUse,
                    cancellationToken
                );
            }

            await client.DisconnectAsync(true, cancellationToken);
            syncRun.Status = ImapSyncStatus.Succeeded;
            syncRun.CompletedAtUtc = DateTime.UtcNow;
            receivingAccount.Status = ReceivingAccountStatus.Active;
            receivingAccount.LastSuccessfulSyncAtUtc = syncRun.CompletedAtUtc;
            receivingAccount.LastConnectedAtUtc = syncRun.CompletedAtUtc;
            receivingAccount.LastError = null;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            syncRun.Status = ImapSyncStatus.Cancelled;
            syncRun.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            syncRun.Status = ImapSyncStatus.Failed;
            syncRun.CompletedAtUtc = DateTime.UtcNow;
            syncRun.ErrorSummary = exception.Message;
            receivingAccount.Status = ReceivingAccountStatus.ConnectionFailed;
            receivingAccount.LastError = exception.Message;
            await db.SaveChangesAsync(cancellationToken);
            throw new KnownException(exception.Message);
        }

        return new ReceivingSynchronizationResult(
            syncRun.Id,
            syncRun.MessagesDiscovered,
            syncRun.MessagesCreated,
            syncRun.MessagesUpdated
        );
    }

    private static List<FolderDefinition> GetSynchronizationFolders(ImapClient client)
    {
        List<FolderDefinition> folders = [new(client.Inbox, ImapMailboxSpecialUse.Inbox)];
        try
        {
            var sentFolder = client.GetFolder(SpecialFolder.Sent);
            if (sentFolder is not null && sentFolder.FullName != client.Inbox.FullName)
                folders.Add(new FolderDefinition(sentFolder, ImapMailboxSpecialUse.Sent));
        }
        catch (FolderNotFoundException)
        {
            // 部分服务商没有声明 Sent 特殊用途，Inbox 仍可正常工作。
        }
        return folders;
    }

    private async Task SynchronizeFolderAsync(
        ReceivingAccount receivingAccount,
        ImapSyncRun syncRun,
        IMailFolder remoteFolder,
        ImapMailboxSpecialUse specialUse,
        CancellationToken cancellationToken
    )
    {
        await remoteFolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
        var localMailbox = await GetOrCreateMailboxAsync(
            receivingAccount.Id,
            remoteFolder,
            specialUse,
            cancellationToken
        );
        var mailboxRun = new ImapMailboxSyncRun
        {
            ReceivingAccountId = receivingAccount.Id,
            ImapSyncRunId = syncRun.Id,
            ImapMailboxId = localMailbox.Id,
            Status = ImapSyncStatus.Running,
        };
        db.ImapMailboxSyncRuns.Add(mailboxRun);

        var checkpoint = await db.ImapMailboxSyncCheckpoints.FirstOrDefaultAsync(
            x => x.ImapMailboxId == localMailbox.Id,
            cancellationToken
        );
        var uidValidity = Convert.ToInt64(remoteFolder.UidValidity);
        if (checkpoint is null)
        {
            checkpoint = new ImapMailboxSyncCheckpoint
            {
                ImapMailboxId = localMailbox.Id,
                UidValidity = uidValidity,
            };
            db.ImapMailboxSyncCheckpoints.Add(checkpoint);
        }
        else if (checkpoint.UidValidity != uidValidity)
        {
            await db
                .IncomingMailLocations.Where(x => x.ImapMailboxId == localMailbox.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.IsPresentOnServer, false),
                    cancellationToken
                );
            checkpoint.UidValidity = uidValidity;
            checkpoint.LastCommittedUid = null;
            checkpoint.LastCommittedModSequence = null;
            checkpoint.SynchronizationGeneration++;
        }

        mailboxRun.StartUid = checkpoint.LastCommittedUid is null
            ? null
            : checkpoint.LastCommittedUid + 1;
        var searchQuery = CreateSearchQuery(checkpoint.LastCommittedUid);
        var uniqueIds = await remoteFolder.SearchAsync(searchQuery, cancellationToken);
        mailboxRun.MessagesDiscovered = uniqueIds.Count;
        syncRun.MessagesDiscovered += uniqueIds.Count;

        if (uniqueIds.Count > 0)
        {
            var summaries = await remoteFolder.FetchAsync(
                uniqueIds,
                MessageSummaryItems.UniqueId
                    | MessageSummaryItems.Envelope
                    | MessageSummaryItems.Flags
                    | MessageSummaryItems.InternalDate
                    | MessageSummaryItems.Size
                    | MessageSummaryItems.References,
                cancellationToken
            );
            foreach (var summary in summaries.OrderBy(x => x.UniqueId.Id))
            {
                var wasCreated = await UpsertMessageAsync(
                    receivingAccount,
                    localMailbox,
                    specialUse,
                    uidValidity,
                    summary,
                    cancellationToken
                );
                if (wasCreated)
                {
                    mailboxRun.LocationsCreated++;
                    syncRun.MessagesCreated++;
                }
                else
                {
                    mailboxRun.LocationsUpdated++;
                    syncRun.MessagesUpdated++;
                }
            }

            checkpoint.LastCommittedUid = uniqueIds.Max(x => Convert.ToInt64(x.Id));
            checkpoint.LastCommittedAtUtc = DateTime.UtcNow;
            mailboxRun.EndUid = checkpoint.LastCommittedUid;
            mailboxRun.CheckpointAdvanced = true;
        }

        localMailbox.UidValidity = uidValidity;
        localMailbox.RemoteMessageCount = remoteFolder.Count;
        localMailbox.RemoteUnreadCount = remoteFolder.Unread;
        localMailbox.LastSuccessfulSyncAtUtc = DateTime.UtcNow;
        localMailbox.LastSyncStatus = ImapSyncStatus.Succeeded;
        localMailbox.LastSyncError = null;
        mailboxRun.Status = ImapSyncStatus.Succeeded;
        await db.SaveChangesAsync(cancellationToken);
        await remoteFolder.CloseAsync(false, cancellationToken);
    }

    private async Task<ImapMailbox> GetOrCreateMailboxAsync(
        long receivingAccountId,
        IMailFolder remoteFolder,
        ImapMailboxSpecialUse specialUse,
        CancellationToken cancellationToken
    )
    {
        var mailbox = await db.ImapMailboxes.FirstOrDefaultAsync(
            x =>
                x.ReceivingAccountId == receivingAccountId
                && x.RemoteFullName == remoteFolder.FullName,
            cancellationToken
        );
        if (mailbox is null)
        {
            mailbox = new ImapMailbox
            {
                ReceivingAccountId = receivingAccountId,
                RemoteFullName = remoteFolder.FullName,
                DisplayName = remoteFolder.Name,
                HierarchyDelimiter =
                    remoteFolder.DirectorySeparator == '\0'
                        ? null
                        : remoteFolder.DirectorySeparator.ToString(),
                SpecialUse = specialUse,
                IsSubscribed = remoteFolder.IsSubscribed,
                IsSynchronizationEnabled = true,
            };
            db.ImapMailboxes.Add(mailbox);
        }
        else
        {
            mailbox.DisplayName = remoteFolder.Name;
            mailbox.SpecialUse = specialUse;
            mailbox.IsSubscribed = remoteFolder.IsSubscribed;
            mailbox.IsSynchronizationEnabled = true;
        }
        mailbox.LastSyncStatus = ImapSyncStatus.Running;
        await db.SaveChangesAsync(cancellationToken);
        return mailbox;
    }

    private async Task<bool> UpsertMessageAsync(
        ReceivingAccount receivingAccount,
        ImapMailbox localMailbox,
        ImapMailboxSpecialUse specialUse,
        long uidValidity,
        IMessageSummary summary,
        CancellationToken cancellationToken
    )
    {
        var uid = Convert.ToInt64(summary.UniqueId.Id);
        var location = await db
            .IncomingMailLocations.Include(x => x.IncomingMailMessage)
            .FirstOrDefaultAsync(
                x =>
                    x.ImapMailboxId == localMailbox.Id
                    && x.UidValidity == uidValidity
                    && x.Uid == uid,
                cancellationToken
            );
        if (location is not null)
        {
            location.Flags = ConvertFlags(summary.Flags);
            location.IsPresentOnServer = true;
            location.LastSynchronizedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }

        var messageIdKey = NormalizeMessageId(summary.Envelope?.MessageId);
        var message = messageIdKey is null
            ? null
            : await db.IncomingMailMessages.FirstOrDefaultAsync(
                x =>
                    x.ReceivingAccountId == receivingAccount.Id
                    && x.InternetMessageIdKey == messageIdKey,
                cancellationToken
            );
        var wasCreated = message is null;
        if (message is null)
        {
            message = new IncomingMailMessage
            {
                ReceivingAccountId = receivingAccount.Id,
                Direction =
                    specialUse == ImapMailboxSpecialUse.Sent
                        ? MailMessageDirection.Outgoing
                        : MailMessageDirection.Incoming,
                InternetMessageId = summary.Envelope?.MessageId,
                InternetMessageIdKey = messageIdKey,
                Subject = summary.Envelope?.Subject,
                SentAtUtc = summary.Envelope?.Date?.UtcDateTime,
                ReceivedAtUtc = summary.InternalDate?.UtcDateTime ?? DateTime.UtcNow,
                Size = summary.Size ?? 0,
                BodyContentStatus = IncomingMailBodyContentStatus.NotRequested,
            };
            if (message.Direction == MailMessageDirection.Outgoing && messageIdKey is not null)
            {
                message.SendingItemId = await db
                    .SendingItems.Where(x => x.InternetMessageIdKey == messageIdKey)
                    .Select(x => (long?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            db.IncomingMailMessages.Add(message);
            AddAddresses(message, summary.Envelope);
            AddReferences(message, summary.Envelope?.InReplyTo, summary.References);
            await db.SaveChangesAsync(cancellationToken);
        }

        db.IncomingMailLocations.Add(
            new IncomingMailLocation
            {
                ReceivingAccountId = receivingAccount.Id,
                IncomingMailMessageId = message.Id,
                ImapMailboxId = localMailbox.Id,
                UidValidity = uidValidity,
                Uid = uid,
                Flags = ConvertFlags(summary.Flags),
                IsPresentOnServer = true,
                FirstSeenAtUtc = DateTime.UtcNow,
                LastSynchronizedAtUtc = DateTime.UtcNow,
            }
        );
        await db.SaveChangesAsync(cancellationToken);
        await conversationIngestionService.IngestAsync(message.Id, cancellationToken);
        return wasCreated;
    }

    private static SearchQuery CreateSearchQuery(long? lastCommittedUid)
    {
        if (lastCommittedUid is null)
            return SearchQuery.DeliveredAfter(DateTime.UtcNow.AddDays(-InitialLookbackDays));
        if (lastCommittedUid >= uint.MaxValue)
            return SearchQuery.All.And(SearchQuery.Not(SearchQuery.All));
        var range = new UniqueIdRange(
            new UniqueId((uint)lastCommittedUid.Value + 1),
            UniqueId.MaxValue
        );
        return SearchQuery.Uids(range);
    }

    private static void AddAddresses(IncomingMailMessage message, Envelope? envelope)
    {
        if (envelope is null)
            return;
        AddAddressList(message, envelope.From, IncomingMailAddressType.From);
        AddAddressList(message, envelope.Sender, IncomingMailAddressType.Sender);
        AddAddressList(message, envelope.ReplyTo, IncomingMailAddressType.ReplyTo);
        AddAddressList(message, envelope.To, IncomingMailAddressType.To);
        AddAddressList(message, envelope.Cc, IncomingMailAddressType.Cc);
        AddAddressList(message, envelope.Bcc, IncomingMailAddressType.Bcc);
    }

    private static void AddAddressList(
        IncomingMailMessage message,
        InternetAddressList? addresses,
        IncomingMailAddressType addressType
    )
    {
        if (addresses is null)
            return;
        var position = 0;
        foreach (var mailboxAddress in addresses.Mailboxes)
        {
            message.Addresses.Add(
                new IncomingMailAddress
                {
                    AddressType = addressType,
                    Email = mailboxAddress.Address,
                    DisplayName = mailboxAddress.Name,
                    Position = position++,
                }
            );
        }
    }

    private static void AddReferences(
        IncomingMailMessage message,
        string? inReplyTo,
        MessageIdList? references
    )
    {
        if (!string.IsNullOrWhiteSpace(inReplyTo))
        {
            message.References.Add(
                new IncomingMailReference
                {
                    ReferenceType = IncomingMailReferenceType.InReplyTo,
                    InternetMessageId = inReplyTo,
                    Position = 0,
                }
            );
        }
        if (references is null)
            return;
        var position = 0;
        foreach (var reference in references)
        {
            message.References.Add(
                new IncomingMailReference
                {
                    ReferenceType = IncomingMailReferenceType.References,
                    InternetMessageId = reference,
                    Position = position++,
                }
            );
        }
    }

    private static ImapMessageFlags ConvertFlags(MessageFlags? flags)
    {
        var source = flags ?? MessageFlags.None;
        var result = ImapMessageFlags.None;
        if (source.HasFlag(MessageFlags.Seen))
            result |= ImapMessageFlags.Seen;
        if (source.HasFlag(MessageFlags.Answered))
            result |= ImapMessageFlags.Answered;
        if (source.HasFlag(MessageFlags.Flagged))
            result |= ImapMessageFlags.Flagged;
        if (source.HasFlag(MessageFlags.Draft))
            result |= ImapMessageFlags.Draft;
        if (source.HasFlag(MessageFlags.Deleted))
            result |= ImapMessageFlags.Deleted;
        if (source.HasFlag(MessageFlags.Recent))
            result |= ImapMessageFlags.Recent;
        return result;
    }

    private static string? NormalizeMessageId(string? messageId) =>
        string.IsNullOrWhiteSpace(messageId) ? null : messageId.Trim().ToLowerInvariant();

    private sealed record FolderDefinition(IMailFolder Folder, ImapMailboxSpecialUse SpecialUse);
}
