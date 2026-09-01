using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.DB.SQL.Core.Todos;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 根据账号、外部参与人和 RFC 引用关系维护会话及待办分支归属。
/// </summary>
public sealed class MailConversationIngestionService(SqlContext db)
    : IMailConversationIngestionService,
        IScopedService<IMailConversationIngestionService>
{
    public async Task IngestAsync(
        long mailboxMessageId,
        CancellationToken cancellationToken = default
    )
    {
        var mailboxMessage = await db
            .IncomingMailMessages.Include(x => x.ReceivingAccount)
            .ThenInclude(x => x.EmailAccount)
            .Include(x => x.Addresses)
            .Include(x => x.References)
            .Include(x => x.Locations)
            .FirstOrDefaultAsync(x => x.Id == mailboxMessageId, cancellationToken);
        if (mailboxMessage is null)
            return;

        var existingTimelineMessage = await FindExistingTimelineMessageAsync(
            mailboxMessage,
            cancellationToken
        );
        if (existingTimelineMessage is not null)
        {
            existingTimelineMessage.IncomingMailMessageId = mailboxMessage.Id;
            existingTimelineMessage.OccurredAtUtc = ResolveOccurredAt(mailboxMessage);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var emailAccount = mailboxMessage.ReceivingAccount.EmailAccount;
        var participantAddresses = GetExternalParticipants(mailboxMessage, emailAccount.Email);
        if (participantAddresses.Count == 0)
            return;

        var contacts = await GetOrCreateContactsAsync(
            emailAccount.UserId,
            emailAccount.OrganizationId,
            participantAddresses,
            ResolveOccurredAt(mailboxMessage),
            cancellationToken
        );
        var referencedTimelineMessage = await FindReferencedTimelineMessageAsync(
            mailboxMessage,
            cancellationToken
        );
        var conversation = referencedTimelineMessage?.MailConversation;
        if (conversation is null)
        {
            var participantSetKey = CreateParticipantSetKey(
                contacts.Select(x => x.NormalizedEmail)
            );
            conversation = await db
                .MailConversations.Include(x => x.Participants)
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == emailAccount.UserId
                        && x.EmailAccountId == emailAccount.Id
                        && x.ParticipantSetKey == participantSetKey,
                    cancellationToken
                );
            if (conversation is null)
            {
                conversation = new MailConversation
                {
                    UserId = emailAccount.UserId,
                    OrganizationId = emailAccount.OrganizationId,
                    EmailAccountId = emailAccount.Id,
                    ConversationType =
                        contacts.Count == 1
                            ? MailConversationType.Direct
                            : MailConversationType.Group,
                    ParticipantSetKey = participantSetKey,
                    DisplayTitle = CreateConversationTitle(contacts),
                };
                db.MailConversations.Add(conversation);
            }
        }

        AddMissingParticipants(conversation, contacts, ResolveOccurredAt(mailboxMessage));
        var timelineMessage = new MailConversationMessage
        {
            MailConversation = conversation,
            IncomingMailMessageId = mailboxMessage.Id,
            SendingItemId = mailboxMessage.SendingItemId,
            SourceKey = mailboxMessage.SendingItemId is > 0
                ? $"sending:{mailboxMessage.SendingItemId}"
                : $"mailbox:{mailboxMessage.Id}",
            Direction = mailboxMessage.Direction,
            OccurredAtUtc = ResolveOccurredAt(mailboxMessage),
            IsRead =
                mailboxMessage.Direction == MailMessageDirection.Outgoing || IsSeen(mailboxMessage),
            ReplyToConversationMessageId = referencedTimelineMessage?.Id,
        };
        db.MailConversationMessages.Add(timelineMessage);

        conversation.LastMessageAtUtc = timelineMessage.OccurredAtUtc;
        conversation.LastMessagePreview = mailboxMessage.Subject;
        if (mailboxMessage.Direction == MailMessageDirection.Incoming && !timelineMessage.IsRead)
            conversation.UnreadCount++;

        if (referencedTimelineMessage is not null)
            await AttachReferencedTodoBranchAsync(referencedTimelineMessage.Id, timelineMessage);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<MailConversationMessage?> FindExistingTimelineMessageAsync(
        IncomingMailMessage mailboxMessage,
        CancellationToken cancellationToken
    )
    {
        if (mailboxMessage.SendingItemId is not > 0)
            return null;
        return await db.MailConversationMessages.FirstOrDefaultAsync(
            x => x.SendingItemId == mailboxMessage.SendingItemId,
            cancellationToken
        );
    }

    private async Task<MailConversationMessage?> FindReferencedTimelineMessageAsync(
        IncomingMailMessage mailboxMessage,
        CancellationToken cancellationToken
    )
    {
        var referenceKeys = mailboxMessage
            .References.OrderByDescending(x => x.Position)
            .Select(x => NormalizeMessageId(x.InternetMessageId))
            .Where(x => x is not null)
            .Cast<string>()
            .Distinct()
            .ToList();
        if (referenceKeys.Count == 0)
            return null;

        return await db
            .MailConversationMessages.Include(x => x.MailConversation)
            .Where(x =>
                (
                    x.SendingItem != null
                    && x.SendingItem.InternetMessageIdKey != null
                    && referenceKeys.Contains(x.SendingItem.InternetMessageIdKey)
                )
                || (
                    x.IncomingMailMessage != null
                    && x.IncomingMailMessage.InternetMessageIdKey != null
                    && referenceKeys.Contains(x.IncomingMailMessage.InternetMessageIdKey)
                )
            )
            .OrderByDescending(x => x.OccurredAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<MailContact>> GetOrCreateContactsAsync(
        long userId,
        long organizationId,
        IReadOnlyCollection<MailboxAddress> addresses,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken
    )
    {
        var normalizedEmails = addresses
            .Select(x => x.Address.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
        var contacts = await db
            .MailContacts.Where(x =>
                x.UserId == userId && normalizedEmails.Contains(x.NormalizedEmail)
            )
            .ToListAsync(cancellationToken);

        foreach (var address in addresses)
        {
            var normalizedEmail = address.Address.Trim().ToLowerInvariant();
            var contact = contacts.FirstOrDefault(x => x.NormalizedEmail == normalizedEmail);
            if (contact is null)
            {
                contact = new MailContact
                {
                    UserId = userId,
                    OrganizationId = organizationId,
                    Email = address.Address,
                    DisplayName = address.Name,
                    LastInteractionAtUtc = occurredAtUtc,
                };
                contacts.Add(contact);
                db.MailContacts.Add(contact);
                continue;
            }

            contact.LastInteractionAtUtc =
                occurredAtUtc > contact.LastInteractionAtUtc
                    ? occurredAtUtc
                    : contact.LastInteractionAtUtc;
            if (!string.IsNullOrWhiteSpace(address.Name))
                contact.DisplayName = address.Name;
        }

        return contacts;
    }

    private static List<MailboxAddress> GetExternalParticipants(
        IncomingMailMessage mailboxMessage,
        string ownEmail
    )
    {
        var ownNormalizedEmail = ownEmail.Trim().ToLowerInvariant();
        return mailboxMessage
            .Addresses.Where(x =>
                x.AddressType
                    is IncomingMailAddressType.From
                        or IncomingMailAddressType.ReplyTo
                        or IncomingMailAddressType.To
                        or IncomingMailAddressType.Cc
                        or IncomingMailAddressType.Bcc
                && !string.Equals(x.Email, ownNormalizedEmail, StringComparison.OrdinalIgnoreCase)
            )
            .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                var address = x.First();
                return new MailboxAddress(address.DisplayName ?? address.Email, address.Email);
            })
            .ToList();
    }

    private static void AddMissingParticipants(
        MailConversation conversation,
        IReadOnlyCollection<MailContact> contacts,
        DateTime occurredAtUtc
    )
    {
        foreach (var contact in contacts)
        {
            var participant = conversation.Participants.FirstOrDefault(x =>
                x.MailContactId == contact.Id && contact.Id > 0
            );
            if (participant is not null)
            {
                participant.IsActive = true;
                participant.LeftAtUtc = null;
                continue;
            }

            conversation.Participants.Add(
                new MailConversationParticipant
                {
                    MailContact = contact,
                    IsActive = true,
                    JoinedAtUtc = occurredAtUtc,
                }
            );
        }
    }

    private async Task AttachReferencedTodoBranchAsync(
        long referencedTimelineMessageId,
        MailConversationMessage timelineMessage
    )
    {
        var branchId = await db
            .TodoMailBranchMessages.Where(x =>
                x.MailConversationMessageId == referencedTimelineMessageId
            )
            .Select(x => (long?)x.TodoMailBranchId)
            .FirstOrDefaultAsync();
        if (branchId is null)
            return;
        db.TodoMailBranchMessages.Add(
            new TodoMailBranchMessage
            {
                TodoMailBranchId = branchId.Value,
                MailConversationMessage = timelineMessage,
            }
        );
    }

    private static bool IsSeen(IncomingMailMessage mailboxMessage) =>
        mailboxMessage.Locations.Any(x => x.Flags.HasFlag(ImapMessageFlags.Seen));

    private static DateTime ResolveOccurredAt(IncomingMailMessage mailboxMessage) =>
        mailboxMessage.SentAtUtc ?? mailboxMessage.ReceivedAtUtc;

    private static string CreateParticipantSetKey(IEnumerable<string> normalizedEmails)
    {
        var canonicalSet = string.Join("\n", normalizedEmails.Order(StringComparer.Ordinal));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalSet)));
    }

    private static string CreateConversationTitle(IReadOnlyCollection<MailContact> contacts) =>
        string.Join(", ", contacts.Select(x => x.DisplayName ?? x.Email));

    private static string? NormalizeMessageId(string? messageId) =>
        string.IsNullOrWhiteSpace(messageId) ? null : messageId.Trim().ToLowerInvariant();
}
