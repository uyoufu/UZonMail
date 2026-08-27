using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Organization;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证发件组按组织内收件箱无效状态过滤收件组成员
/// </summary>
[TestClass]
public sealed class SendingItemsBuilderRecipientValidationStatusTests
{
    [TestMethod]
    public async Task GenerateAndSave_ExcludesAddressInvalidatedByAnotherUserInOrganization()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var organization = new Department
        {
            Id = 1,
            Name = "Organization",
            FullPath = "/1",
            Type = DepartmentType.Organization,
        };
        var currentUser = SendCoreTestEntityFactory.CreateUser(101, organization.Id);
        var sameOrganizationUser = SendCoreTestEntityFactory.CreateUser(102, organization.Id);
        var otherOrganizationUser = SendCoreTestEntityFactory.CreateUser(201, 2);
        var otherOrganization = new Department
        {
            Id = 2,
            Name = "Other organization",
            FullPath = "/2",
            Type = DepartmentType.Organization,
        };
        var recipientContactGroup = new EmailGroup
        {
            Id = 10,
            UserId = currentUser.Id,
            Name = "Recipients",
            Category = EmailGroupCategory.Recipient,
        };
        var otherRecipientContactGroup = new EmailGroup
        {
            Id = 11,
            UserId = otherOrganizationUser.Id,
            Name = "Other recipients",
            Category = EmailGroupCategory.Recipient,
        };
        var sendingGroup = new SendingGroup
        {
            Id = 20,
            UserId = currentUser.Id,
            RecipientContactGroups = [new() { Id = recipientContactGroup.Id }],
        };
        testDatabase.Db.AddRange(
            organization,
            otherOrganization,
            currentUser,
            sameOrganizationUser,
            otherOrganizationUser,
            recipientContactGroup,
            otherRecipientContactGroup,
            sendingGroup,
            new RecipientContact
            {
                UserId = currentUser.Id,
                OrganizationId = organization.Id,
                EmailGroupId = recipientContactGroup.Id,
                Email = "shared@example.com",
                ValidationStatus = RecipientValidationStatus.Valid,
            },
            new RecipientContact
            {
                UserId = currentUser.Id,
                OrganizationId = organization.Id,
                EmailGroupId = recipientContactGroup.Id,
                Email = "valid@example.com",
                ValidationStatus = RecipientValidationStatus.Valid,
            },
            new RecipientContact
            {
                UserId = sameOrganizationUser.Id,
                OrganizationId = organization.Id,
                EmailGroupId = recipientContactGroup.Id,
                Email = "shared@example.com",
                ValidationStatus = RecipientValidationStatus.Invalid,
            },
            new RecipientContact
            {
                UserId = otherOrganizationUser.Id,
                OrganizationId = 2,
                EmailGroupId = otherRecipientContactGroup.Id,
                Email = "valid@example.com",
                ValidationStatus = RecipientValidationStatus.Invalid,
            }
        );
        await testDatabase.Db.SaveChangesAsync();

        var builder = new SendingItemsBuilder(
            testDatabase.Db,
            sendingGroup,
            maxSendingBatchSize: 20,
            allowDuplicateSending: false,
            excelAttachments: new Dictionary<string, FileUsage>(),
            organizationId: organization.Id
        );
        var sendingItems = await builder.GenerateAndSave();

        Assert.HasCount(1, sendingItems);
        Assert.AreEqual("valid@example.com", sendingItems[0].Recipients[0].Email);
        Assert.AreEqual(1, await testDatabase.Db.SendingItems.CountAsync());
    }

    [TestMethod]
    public async Task GenerateAndSave_AllowDuplicateSending_CreatesItemForEveryDuplicateExcelRow()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var organization = new Department
        {
            Id = 1,
            Name = "Organization",
            FullPath = "/1",
            Type = DepartmentType.Organization,
        };
        var user = SendCoreTestEntityFactory.CreateUser(101, organization.Id);
        var recipientContactGroup = new EmailGroup
        {
            Id = 10,
            UserId = user.Id,
            Name = "Recipients",
            Category = EmailGroupCategory.Recipient,
        };
        var sendingGroup = new SendingGroup
        {
            Id = 20,
            UserId = user.Id,
            Data =
            [
                new JObject { ["recipientEmail"] = "recipient@example.com" },
                new JObject { ["recipientEmail"] = "recipient@example.com" },
            ],
        };
        testDatabase.Db.AddRange(
            organization,
            user,
            recipientContactGroup,
            sendingGroup,
            new RecipientContact
            {
                Id = 30,
                UserId = user.Id,
                OrganizationId = organization.Id,
                EmailGroupId = recipientContactGroup.Id,
                Email = "recipient@example.com",
                ValidationStatus = RecipientValidationStatus.Valid,
            }
        );
        await testDatabase.Db.SaveChangesAsync();

        var builder = new SendingItemsBuilder(
            testDatabase.Db,
            sendingGroup,
            maxSendingBatchSize: 20,
            allowDuplicateSending: true,
            excelAttachments: new Dictionary<string, FileUsage>(),
            organizationId: organization.Id
        );

        var sendingItems = await builder.GenerateAndSave();

        Assert.HasCount(2, sendingItems);
        Assert.AreEqual(2, await testDatabase.Db.SendingItems.CountAsync());
    }
}
