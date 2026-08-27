using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Organization;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

internal static class SendCoreTestEntityFactory
{
    internal static User CreateUser(long userId, long organizationId) =>
        new()
        {
            Id = userId,
            UserId = $"user-{userId}",
            Password = "password",
            OrganizationId = organizationId,
            DepartmentId = organizationId,
        };

    internal static SenderEmailAddress CreateSenderAccountAddress(
        long senderAccountId = 20,
        long userId = 30,
        long sendingGroupId = 10,
        SenderEmailAddressType type = SenderEmailAddressType.Shared,
        List<long>? sendingItemIds = null,
        Action<SenderAccount>? configure = null
    )
    {
        var senderAccount = new SenderAccount
        {
            Id = senderAccountId,
            EmailAccount = new EmailAccount
            {
                UserId = userId,
                Email = $"sender-{senderAccountId}@test.com",
                Name = "Sender",
            },
            Protocol = SendingProtocol.Smtp,
            ReplyToEmails = string.Empty,
            Weight = 1,
            Status = SenderAccountStatus.Valid,
        };
        configure?.Invoke(senderAccount);
        return new SenderEmailAddress(
            senderAccount,
            new SenderCredentialSnapshot(
                new SmtpCredentialSnapshot(
                    "smtp.test.com",
                    465,
                    ConnectionSecurity.SSL,
                    senderAccount.Email,
                    "password"
                ),
                null
            ),
            sendingGroupId,
            type,
            sendingItemIds
        );
    }

    internal static PreparedSendItem CreatePreparedItem(
        SenderEmailAddress? senderAccount = null,
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
            UserId = senderAccount?.UserId ?? 30,
            Recipients = [new EmailAddress { Id = 100, Email = "recipient@test.com" }],
            TriedCount = triedCount,
        };
        configureItem?.Invoke(sendingItem);
        var setting = new SendingSetting();
        configureSetting?.Invoke(setting);
        return new PreparedSendItem(
            sendingItem,
            senderAccount ?? CreateSenderAccountAddress(),
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
