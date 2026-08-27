using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

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
                item.SenderAccount.Email = invalidAddress;
                break;
            case "收件人":
                item.SourceItem.Recipients = [new EmailAddress { Email = invalidAddress }];
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

    private static PreparedSendItem CreatePreparedItem(
        List<EmailAddress> recipientContacts,
        string htmlBody
    )
    {
        var senderAccountAddress = SendCoreTestEntityFactory.CreateSenderAccountAddress();
        var sendingItem = new SendingItem { UserId = 30, Recipients = recipientContacts };
        return new PreparedSendItem(
            sendingItem,
            senderAccountAddress,
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
