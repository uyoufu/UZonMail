using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Organization;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证发件项使用独立集合，并统一应用 Excel 行级覆盖规则。
/// </summary>
[TestClass]
public sealed class SendingItemsBuilderRecipientIsolationTests
{
    [TestMethod]
    public async Task GenerateAndSave_ExcelRows_OverrideOrFallbackRecipientCollectionsPerField()
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
        var fileBucket = new FileBucket
        {
            Id = 40,
            BucketName = "Default",
            RootDir = "test-files",
            IsDefault = true,
        };
        var fileCategory = new FileCategory
        {
            Id = 50,
            OwnerUser = user,
            Name = FileCategory.DefaultName,
            IsDefault = true,
        };
        var globalAttachment = CreateFileUsage(
            61,
            user,
            fileCategory,
            fileBucket,
            "global.txt",
            'a'
        );
        var firstRowAttachment = CreateFileUsage(
            62,
            user,
            fileCategory,
            fileBucket,
            "first-row.txt",
            'b'
        );
        var recipientContactGroup = new EmailGroup
        {
            Id = 10,
            UserId = user.Id,
            Name = "Recipients",
            Category = EmailGroupCategory.Recipient,
        };
        var firstRecipientContact = CreateRecipientContact(
            31,
            user.Id,
            organization.Id,
            recipientContactGroup.Id,
            "first@example.com"
        );
        var secondRecipientContact = CreateRecipientContact(
            32,
            user.Id,
            organization.Id,
            recipientContactGroup.Id,
            "second@example.com"
        );
        var sendingGroup = new SendingGroup
        {
            Id = 20,
            UserId = user.Id,
            CcBoxes = [new EmailAddress { Email = "global-cc@example.com" }],
            BccBoxes = [new EmailAddress { Email = "global-bcc@example.com" }],
            Attachments = [globalAttachment],
            Data =
            [
                new JObject
                {
                    ["recipientEmail"] = firstRecipientContact.Email,
                    ["cc"] = "first-row-cc@example.com",
                    ["attachmentNames"] = firstRowAttachment.DisplayName,
                },
                new JObject
                {
                    ["recipientEmail"] = secondRecipientContact.Email,
                    ["bcc"] = "second-row-bcc@example.com",
                },
            ],
        };
        testDatabase.Db.AddRange(
            organization,
            user,
            fileBucket,
            fileCategory,
            globalAttachment,
            firstRowAttachment,
            recipientContactGroup,
            firstRecipientContact,
            secondRecipientContact,
            sendingGroup
        );
        await testDatabase.Db.SaveChangesAsync();

        var builder = CreateBuilder(
            testDatabase,
            sendingGroup,
            organization.Id,
            new Dictionary<string, FileUsage>
            {
                [FileStoreService.NormalizeDisplayName(firstRowAttachment.DisplayName)] =
                    firstRowAttachment,
            }
        );

        var sendingItems = await builder.GenerateAndSave();

        Assert.HasCount(2, sendingItems);
        var firstItem = sendingItems.Single(x =>
            x.Recipients.Single().Email == firstRecipientContact.Email
        );
        var secondItem = sendingItems.Single(x =>
            x.Recipients.Single().Email == secondRecipientContact.Email
        );
        CollectionAssert.AreEqual(
            new[] { "first-row-cc@example.com" },
            firstItem.CC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            firstItem.BCC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            secondItem.CC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "second-row-bcc@example.com" },
            secondItem.BCC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { firstRowAttachment.Id },
            firstItem.Attachments!.Select(x => x.Id).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { globalAttachment.Id },
            secondItem.Attachments!.Select(x => x.Id).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            sendingGroup.CcBoxes!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            sendingGroup.BccBoxes!.Select(x => x.Email).ToArray()
        );
        AssertIndependentCollections(sendingGroup, firstItem, secondItem);
        Assert.AreNotSame(sendingGroup.BccBoxes![0], firstItem.BCC![0]);
        Assert.AreNotSame(sendingGroup.CcBoxes![0], secondItem.CC![0]);

        var recipientContactRelations = await testDatabase
            .Db.SendingItemRecipients.AsNoTracking()
            .ToListAsync();
        CollectionAssert.AreEqual(
            new[] { "first-row-cc@example.com" },
            GetRoleEmails(recipientContactRelations, firstItem.Id, RecipientRole.CC)
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            GetRoleEmails(recipientContactRelations, firstItem.Id, RecipientRole.BCC)
        );
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            GetRoleEmails(recipientContactRelations, secondItem.Id, RecipientRole.CC)
        );
        CollectionAssert.AreEqual(
            new[] { "second-row-bcc@example.com" },
            GetRoleEmails(recipientContactRelations, secondItem.Id, RecipientRole.BCC)
        );
        Assert.AreEqual(
            "first@example.com,first-row-cc@example.com,global-bcc@example.com",
            firstItem.RecipientEmails
        );
        Assert.AreEqual(
            "second@example.com,global-cc@example.com,second-row-bcc@example.com",
            secondItem.RecipientEmails
        );
    }

    [TestMethod]
    public async Task GenerateAndSave_BatchItems_CopyGlobalRecipientCollectionsPerBatch()
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
        var senderAccountGroup = new EmailGroup
        {
            Id = 11,
            UserId = user.Id,
            Name = "Senders",
            Category = EmailGroupCategory.Sender,
        };
        var senderAccount = new SenderAccount
        {
            Id = 21,
            EmailAccount = new EmailAccount
            {
                UserId = user.Id,
                OrganizationId = organization.Id,
                Email = "sender@example.com",
            },
            EmailGroupId = senderAccountGroup.Id,
            Protocol = SendingProtocol.Smtp,
            Status = SenderAccountStatus.Valid,
        };
        var firstRecipientContact = CreateRecipientContact(
            31,
            user.Id,
            organization.Id,
            recipientContactGroup.Id,
            "first@example.com"
        );
        var secondRecipientContact = CreateRecipientContact(
            32,
            user.Id,
            organization.Id,
            recipientContactGroup.Id,
            "second@example.com"
        );
        var sendingGroup = new SendingGroup
        {
            Id = 20,
            UserId = user.Id,
            SendBatch = true,
            SenderAccounts = [senderAccount],
            Recipients =
            [
                new EmailAddress
                {
                    Id = firstRecipientContact.Id,
                    Email = firstRecipientContact.Email
                },
                new EmailAddress
                {
                    Id = secondRecipientContact.Id,
                    Email = secondRecipientContact.Email
                },
            ],
            CcBoxes = [new EmailAddress { Email = "global-cc@example.com" }],
            BccBoxes = [new EmailAddress { Email = "global-bcc@example.com" }],
        };
        testDatabase.Db.AddRange(
            organization,
            user,
            recipientContactGroup,
            senderAccountGroup,
            senderAccount,
            firstRecipientContact,
            secondRecipientContact,
            sendingGroup
        );
        await testDatabase.Db.SaveChangesAsync();
        var builder = CreateBuilder(
            testDatabase,
            sendingGroup,
            organization.Id,
            maxSendingBatchSize: 3
        );

        var sendingItems = await builder.GenerateAndSave();

        Assert.HasCount(2, sendingItems);
        Assert.IsTrue(sendingItems.All(x => x.IsSendingBatch));
        AssertIndependentCollections(sendingGroup, sendingItems[0], sendingItems[1]);
        Assert.AreNotSame(sendingItems[0].CC![0], sendingItems[1].CC![0]);
        Assert.AreNotSame(sendingItems[0].BCC![0], sendingItems[1].BCC![0]);
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            sendingItems[0].CC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            sendingItems[1].CC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            sendingItems[0].BCC!.Select(x => x.Email).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            sendingItems[1].BCC!.Select(x => x.Email).ToArray()
        );
    }

    private static SendingItemsBuilder CreateBuilder(
        SqliteTestDatabase testDatabase,
        SendingGroup sendingGroup,
        long organizationId,
        IReadOnlyDictionary<string, FileUsage>? excelAttachments = null,
        int maxSendingBatchSize = 20
    ) =>
        new(
            testDatabase.Db,
            sendingGroup,
            maxSendingBatchSize,
            allowDuplicateSending: false,
            excelAttachments ?? new Dictionary<string, FileUsage>(),
            organizationId
        );

    private static FileUsage CreateFileUsage(
        long fileUsageId,
        User user,
        FileCategory fileCategory,
        FileBucket fileBucket,
        string displayName,
        char hashCharacter
    ) =>
        new()
        {
            Id = fileUsageId,
            OwnerUser = user,
            Category = fileCategory,
            FileName = displayName,
            DisplayName = displayName,
            DisplayNameKey = FileStoreService.NormalizeDisplayName(displayName),
            FileObject = new FileObject
            {
                Id = fileUsageId + 10,
                FileBucket = fileBucket,
                Path = displayName,
                Sha256 = new string(hashCharacter, 64),
                Size = 1,
                StorageState = FileObjectStorageState.Ready,
            },
        };

    private static RecipientContact CreateRecipientContact(
        long recipientContactId,
        long userId,
        long organizationId,
        long emailGroupId,
        string email
    ) =>
        new()
        {
            Id = recipientContactId,
            UserId = userId,
            OrganizationId = organizationId,
            EmailGroupId = emailGroupId,
            Email = email,
            ValidationStatus = RecipientValidationStatus.Valid,
        };

    private static void AssertIndependentCollections(
        SendingGroup sendingGroup,
        SendingItem firstItem,
        SendingItem secondItem
    )
    {
        Assert.AreNotSame(sendingGroup.CcBoxes, firstItem.CC);
        Assert.AreNotSame(sendingGroup.CcBoxes, secondItem.CC);
        Assert.AreNotSame(firstItem.CC, secondItem.CC);
        Assert.AreNotSame(sendingGroup.BccBoxes, firstItem.BCC);
        Assert.AreNotSame(sendingGroup.BccBoxes, secondItem.BCC);
        Assert.AreNotSame(firstItem.BCC, secondItem.BCC);
        Assert.AreNotSame(sendingGroup.Attachments, firstItem.Attachments);
        Assert.AreNotSame(sendingGroup.Attachments, secondItem.Attachments);
        Assert.AreNotSame(firstItem.Attachments, secondItem.Attachments);
    }

    private static string[] GetRoleEmails(
        IEnumerable<SendingItemRecipient> recipientContactRelations,
        long sendingItemId,
        RecipientRole recipientContactRole
    ) =>
        recipientContactRelations
            .Where(x => x.SendingItemId == sendingItemId && x.Role == recipientContactRole)
            .Select(x => x.RecipientEmail!)
            .ToArray();
}
