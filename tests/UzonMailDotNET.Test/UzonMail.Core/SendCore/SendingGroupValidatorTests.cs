using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Database.Validators;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证 Excel 收件人完整性校验与重复收件人策略相互独立。
/// </summary>
[TestClass]
public sealed class SendingGroupValidatorTests
{
    [TestMethod]
    public void Validate_DuplicateInboxRows_DoesNotReportMissingInbox()
    {
        var excelData = CreateExcelData(
            new JObject { ["inbox"] = "recipient@example.com" },
            new JObject { ["inbox"] = "recipient@example.com" }
        );

        var excelDataInfo = new ExcelDataInfo(excelData);
        var validationResult = new SendingGroupValidator().Validate(CreateSendingGroup(excelData));

        Assert.AreEqual(ExcelDataStatus.All, excelDataInfo.InboxStatus);
        Assert.HasCount(1, excelDataInfo.InboxSet);
        Assert.IsTrue(validationResult.IsValid);
    }

    [TestMethod]
    public void Validate_MissingInboxRows_ReportsMissingRowNumbers()
    {
        var excelData = CreateExcelData(
            new JObject { ["inbox"] = "recipient@example.com" },
            new JObject { ["inbox"] = "  " },
            new JValue("invalid-row")
        );

        var validationResult = new SendingGroupValidator().Validate(CreateSendingGroup(excelData));

        Assert.IsFalse(validationResult.IsValid);
        Assert.HasCount(1, validationResult.Errors);
        Assert.AreEqual("Excel 数据第 2、3 行缺少 inbox (收件人邮箱)", validationResult.Errors[0].ErrorMessage);
    }

    private static JArray CreateExcelData(params JToken[] rows) => new(rows);

    private static SendingGroup CreateSendingGroup(JArray excelData) =>
        new()
        {
            Subjects = "Test subject",
            Body = "Test body",
            Data = excelData,
            Outboxes = [new Outbox { Email = "sender@example.com" }],
        };
}
