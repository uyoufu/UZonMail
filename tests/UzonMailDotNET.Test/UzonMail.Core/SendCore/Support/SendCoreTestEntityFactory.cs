using UzonMail.CorePlugin.Services.Encrypt.Models;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Extensions;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

internal static class SendCoreTestEntityFactory
{
    internal static EncryptParams Encryption { get; } = new();

    internal static OutboxEmailAddress CreateOutboxAddress(
        long outboxId = 20,
        long userId = 30,
        long sendingGroupId = 10,
        OutboxEmailAddressType type = OutboxEmailAddressType.Shared,
        List<long>? sendingItemIds = null,
        Action<Outbox>? configure = null
    )
    {
        var outbox = new Outbox
        {
            Id = outboxId,
            UserId = userId,
            Email = $"sender-{outboxId}@test.com",
            Name = "Sender",
            Password = "password".AES(Encryption.Key, Encryption.Iv),
            SmtpHost = "smtp.test.com",
            SmtpPort = 465,
            ReplyToEmails = string.Empty,
            Weight = 1,
            IsValid = true,
            Status = OutboxStatus.Valid,
        };
        configure?.Invoke(outbox);
        return new OutboxEmailAddress(
            outbox,
            sendingGroupId,
            Encryption,
            type,
            sendingItemIds
        );
    }

    internal static PreparedSendItem CreatePreparedItem(
        OutboxEmailAddress? outbox = null,
        long sendingItemId = 1,
        long sendingGroupId = 10,
        int triedCount = 0,
        Action<SendingItem>? configureItem = null,
        Action<SendingSetting>? configureSetting = null
    )
    {
        var sendingItem = new SendingItem
        {
            Id = sendingItemId,
            SendingGroupId = sendingGroupId,
            UserId = outbox?.UserId ?? 30,
            Inboxes = [new EmailAddress { Id = 100, Email = "recipient@test.com" }],
            TriedCount = triedCount,
        };
        configureItem?.Invoke(sendingItem);
        var setting = new SendingSetting();
        configureSetting?.Invoke(setting);
        return new PreparedSendItem(
            sendingItem,
            outbox ?? CreateOutboxAddress(),
            null,
            "subject",
            "<p>body</p>",
            [],
            [],
            [],
            setting
        );
    }
}
