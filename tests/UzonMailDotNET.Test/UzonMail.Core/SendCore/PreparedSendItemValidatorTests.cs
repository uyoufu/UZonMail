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

    [TestMethod]
    [DataRow("发件箱", "友件")]
    [DataRow("收件人", "友件")]
    [DataRow("抄送", "友件")]
    [DataRow("密送", "友件")]
    [DataRow("回复地址", "友件")]
    [DataRow("收件人", "名称 <to@test.com>")]
    public void Validate_ReturnsStronglyTypedFailureForInvalidMimeAddress(
        string addressRole,
        string invalidAddress
    )
    {
        var item = CreatePreparedItem([new EmailAddress { Email = "to@test.com" }], "body");

        switch (addressRole)
        {
            case "发件箱":
                item.Outbox.Email = invalidAddress;
                break;
            case "收件人":
                item.SourceItem.Inboxes = [new EmailAddress { Email = invalidAddress }];
                break;
            case "抄送":
                item.SourceItem.CC = [new EmailAddress { Email = invalidAddress }];
                break;
            case "密送":
                item.SourceItem.BCC = [new EmailAddress { Email = invalidAddress }];
                break;
            case "回复地址":
                item = item with { ReplyToEmails = [invalidAddress] };
                break;
        }

        var result = _validator.Validate(item);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(SendItemValidationFailure.InvalidEmailAddress, result.Failure);
        StringAssert.Contains(result.Message, addressRole);
        StringAssert.Contains(result.Message, invalidAddress);
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
