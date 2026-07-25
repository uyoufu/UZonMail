using UzonMail.CorePlugin.Services.Encrypt.Models;
using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Extensions;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class PreparedSendItemValidatorTests
{
    private readonly PreparedSendItemValidator _validator = new();

    [TestMethod]
    public void Validate_ReturnsStronglyTypedFailureForMissingRecipient()
    {
        var item = CreatePreparedItem([], "body");
        var result = _validator.Validate(item);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(SendItemValidationFailure.MissingRecipients, result.Failure);
    }

    [TestMethod]
    public void Validate_ReturnsStronglyTypedFailureForMissingBody()
    {
        var item = CreatePreparedItem([new EmailAddress { Email = "to@test.com" }], "");
        var result = _validator.Validate(item);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(SendItemValidationFailure.MissingBody, result.Failure);
    }

    [TestMethod]
    public void Validate_AcceptsCompletePreparedItem()
    {
        var item = CreatePreparedItem([new EmailAddress { Email = "to@test.com" }], "body");
        Assert.IsTrue(_validator.Validate(item).IsValid);
    }

    private static PreparedSendItem CreatePreparedItem(List<EmailAddress> inboxes, string htmlBody)
    {
        var encryption = new EncryptParams();
        var outbox = new Outbox
        {
            Id = 20,
            UserId = 30,
            Email = "from@test.com",
            Password = "password".AES(encryption.Key, encryption.Iv),
        };
        var outboxAddress = new OutboxEmailAddress(
            outbox,
            10,
            encryption,
            OutboxEmailAddressType.Shared
        );
        var sendingItem = new SendingItem { UserId = 30, Inboxes = inboxes };
        return new PreparedSendItem(
            sendingItem,
            outboxAddress,
            null,
            "subject",
            htmlBody,
            [],
            [],
            [],
            new SendingSetting()
        );
    }
}
