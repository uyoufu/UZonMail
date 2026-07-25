using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.EmailDecorator;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class FieldsDecoratorTests
{
    [TestMethod]
    public async Task Decorate_UsesExplicitVariablesAndAddressSnapshots()
    {
        var sendingItem = new SendingItem
        {
            Inboxes = [new EmailAddress { Email = "to@test.com", Name = "Recipient" }],
        };
        var variables = new SendingItemExcelData(new JObject { ["customField"] = "custom-value" });
        var context = new EmailDecoratorParams(
            new SendingSetting(),
            sendingItem,
            variables,
            new Outbox { Email = "from@test.com", Name = "Sender" },
            "subject",
            "body"
        );

        var result = await new FieldsDecorator().StartDecorating(
            context,
            "{{ inbox }}|{{ inboxName }}|{{ customField }}"
        );

        Assert.AreEqual("to@test.com|Recipient|custom-value", result);
    }
}
