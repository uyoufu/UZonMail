using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证邮件控制器请求 DTO 只映射允许客户端写入的字段。
/// </summary>
[TestClass]
public sealed class EmailRequestDtoMapperTests
{
    [TestMethod]
    public void CreateOutboxDto_MapsWritableFieldsAndLeavesServerStateEmpty()
    {
        var entity = new CreateOutboxDto
        {
            EmailGroupId = 10,
            Email = " sender@example.com ",
            Name = "Sender",
            Description = "Primary account",
            Remark = "Imported",
            Type = OutboxType.MsGraph,
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            UserName = "smtp-user",
            Password = "secret",
            ConnectionSecurity = ConnectionSecurity.StartTLS,
            ProxyId = 20,
            MaxSendCountPerDay = 100,
            ReplyToEmails = "reply@example.com",
            Weight = 3,
        }.ToEntity();

        Assert.AreEqual(10, entity.EmailGroupId);
        Assert.AreEqual("sender@example.com", entity.Email);
        Assert.AreEqual("Sender", entity.Name);
        Assert.AreEqual("Imported", entity.Remark);
        Assert.AreEqual(OutboxType.MsGraph, entity.Type);
        Assert.AreEqual("smtp.example.com", entity.SmtpHost);
        Assert.AreEqual(587, entity.SmtpPort);
        Assert.AreEqual(ConnectionSecurity.StartTLS, entity.ConnectionSecurity);
        Assert.AreEqual(100, entity.MaxSendCountPerDay);
        Assert.AreEqual(3, entity.Weight);
        Assert.AreEqual(0, entity.UserId);
        Assert.AreEqual(0, entity.SentTotalToday);
        Assert.IsFalse(entity.IsValid);
    }

    [TestMethod]
    public void InboxDtos_MapOperationSpecificFields()
    {
        var created = new CreateInboxDto
        {
            EmailGroupId = 11,
            Email = "inbox@example.com",
            Name = "Inbox",
            Description = "Created",
            Remark = "Remark",
            MinInboxCooldownHours = 24,
        }.ToEntity();
        var ungrouped = new CreateUngroupedInboxDto
        {
            Email = "ungrouped@example.com",
            MinInboxCooldownHours = 12,
        }.ToEntity(12);
        var updated = new UpdateInboxDto
        {
            Email = "updated@example.com",
            Name = "Updated",
            Description = "Changed",
            MinInboxCooldownHours = 6,
        }.ToEntity();

        Assert.AreEqual(11, created.EmailGroupId);
        Assert.AreEqual("Remark", created.Remark);
        Assert.AreEqual(24, created.MinInboxCooldownHours);
        Assert.AreEqual(0, created.UserId);
        Assert.AreEqual(0, created.OrganizationId);
        Assert.AreEqual(12, ungrouped.EmailGroupId);
        Assert.AreEqual("ungrouped@example.com", ungrouped.Email);
        Assert.AreEqual("updated@example.com", updated.Email);
        Assert.AreEqual(6, updated.MinInboxCooldownHours);
    }

    [TestMethod]
    public void ScheduleEmailDto_MapsIdsAddressesAndScheduleDate()
    {
        var scheduleDate = new DateTime(2026, 8, 1, 10, 30, 0, DateTimeKind.Utc);
        var entity = new ScheduleEmailDto
        {
            Subjects = "Subject",
            TemplateIds = [1, 2],
            Body = "Body",
            OutboxIds = [3],
            OutboxGroupIds = [4],
            Inboxes = [new EmailAddressDto { Email = "to@example.com", Name = "To" }],
            InboxGroupIds = [5],
            CcBoxes = [new EmailAddressDto { Email = "cc@example.com" }],
            BccBoxes = [new EmailAddressDto { Email = "bcc@example.com" }],
            AttachmentIds = [6],
            Data = JArray.Parse("[{\"key\":\"value\"}]"),
            SendBatch = true,
            ProxyIds = [7],
            ScheduleDate = scheduleDate,
        }.ToEntity();

        CollectionAssert.AreEqual(
            new long[] { 1, 2 },
            entity.Templates!.Select(x => x.Id).ToArray()
        );
        CollectionAssert.AreEqual(new long[] { 3 }, entity.Outboxes.Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(
            new long[] { 4 },
            entity.OutboxGroups!.Select(x => x.Id).ToArray()
        );
        CollectionAssert.AreEqual(
            new long[] { 5 },
            entity.InboxGroups!.Select(x => x.Id).ToArray()
        );
        CollectionAssert.AreEqual(
            new long[] { 6 },
            entity.Attachments!.Select(x => x.__fileUsageId).ToArray()
        );
        Assert.AreEqual("to@example.com", entity.Inboxes.Single().Email);
        Assert.AreEqual("cc@example.com", entity.CcBoxes!.Single().Email);
        Assert.AreEqual("bcc@example.com", entity.BccBoxes!.Single().Email);
        Assert.AreEqual(scheduleDate, entity.ScheduleDate);
        Assert.IsTrue(entity.SendBatch);
        CollectionAssert.AreEqual(new long[] { 7 }, entity.ProxyIds!.ToArray());
    }

    [TestMethod]
    public void SendEmailNowDto_UsesImmediateScheduleDate()
    {
        var entity = new SendEmailNowDto { Subjects = "Subject" }.ToEntity();

        Assert.AreEqual(DateTime.MinValue, entity.ScheduleDate);
    }

    [TestMethod]
    public void SendEmailNowDto_NullCollectionsMapToEmptyCollections()
    {
        var entity = new SendEmailNowDto
        {
            TemplateIds = null!,
            OutboxIds = null!,
            OutboxGroupIds = null!,
            Inboxes = null!,
            InboxGroupIds = null!,
            CcBoxes = null!,
            BccBoxes = null!,
            AttachmentIds = null!,
            ProxyIds = null!,
        }.ToEntity();

        Assert.IsEmpty(entity.Templates!);
        Assert.IsEmpty(entity.Outboxes);
        Assert.IsEmpty(entity.OutboxGroups!);
        Assert.IsEmpty(entity.Inboxes);
        Assert.IsEmpty(entity.InboxGroups!);
        Assert.IsEmpty(entity.CcBoxes!);
        Assert.IsEmpty(entity.BccBoxes!);
        Assert.IsEmpty(entity.Attachments!);
        Assert.IsEmpty(entity.ProxyIds!);
    }

    [TestMethod]
    public void OtherRequestDtos_MapOnlyTheirBusinessFields()
    {
        var createdGroup = new CreateEmailGroupDto
        {
            Type = EmailGroupType.OutBox,
            Icon = "group",
            Name = "Group",
            Description = "Description",
            Order = 8,
        }.ToEntity();
        var updatedGroup = new UpdateEmailGroupDto
        {
            Name = "Updated group",
            Description = "Updated description",
            Order = 9,
        }.ToEntity();
        var template = new UpsertEmailTemplateDto
        {
            Id = 30,
            Name = "Template",
            Description = "Template description",
            Content = "Content",
        }.ToEntity();
        var smtpInfo = new UpdateSmtpInfoDto
        {
            Domain = "example.com",
            Host = "smtp.example.com",
            Port = 465,
            ConnectionSecurity = ConnectionSecurity.SSL,
            EnableSSL = true,
        }.ToEntity();

        Assert.AreEqual(EmailGroupType.OutBox, createdGroup.Type);
        Assert.AreEqual("group", createdGroup.Icon);
        Assert.AreEqual(0, createdGroup.UserId);
        Assert.AreEqual("Updated group", updatedGroup.Name);
        Assert.AreEqual(9, updatedGroup.Order);
        Assert.AreEqual(30, template.Id);
        Assert.AreEqual("Template description", template.Description);
        Assert.AreEqual(0, template.UserId);
        Assert.AreEqual("example.com", smtpInfo.Domain);
        Assert.AreEqual(465, smtpInfo.Port);
        Assert.IsTrue(smtpInfo.EnableSSL);
    }
}
