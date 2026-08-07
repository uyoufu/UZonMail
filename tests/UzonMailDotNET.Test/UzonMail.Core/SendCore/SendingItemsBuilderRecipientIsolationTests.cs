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
        var inboxGroup = new EmailGroup
        {
            Id = 10,
            UserId = user.Id,
            Name = "Recipients",
            Type = EmailGroupType.InBox,
        };
        var firstInbox = CreateInbox(
            31,
            user.Id,
            organization.Id,
            inboxGroup.Id,
            "first@example.com"
        );
        var secondInbox = CreateInbox(
            32,
            user.Id,
            organization.Id,
            inboxGroup.Id,
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
                    ["inbox"] = firstInbox.Email,
                    ["cc"] = "first-row-cc@example.com",
                    ["attachmentNames"] = firstRowAttachment.DisplayName,
                },
                new JObject
                {
                    ["inbox"] = secondInbox.Email,
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
            inboxGroup,
            firstInbox,
            secondInbox,
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
        var firstItem = sendingItems.Single(x => x.Inboxes.Single().Email == firstInbox.Email);
        var secondItem = sendingItems.Single(x => x.Inboxes.Single().Email == secondInbox.Email);
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

        var inboxRelations = await testDatabase.Db.SendingItemInboxes.AsNoTracking().ToListAsync();
        CollectionAssert.AreEqual(
            new[] { "first-row-cc@example.com" },
            GetRoleEmails(inboxRelations, firstItem.Id, InboxRole.CC)
        );
        CollectionAssert.AreEqual(
            new[] { "global-bcc@example.com" },
            GetRoleEmails(inboxRelations, firstItem.Id, InboxRole.BCC)
        );
        CollectionAssert.AreEqual(
            new[] { "global-cc@example.com" },
            GetRoleEmails(inboxRelations, secondItem.Id, InboxRole.CC)
        );
        CollectionAssert.AreEqual(
            new[] { "second-row-bcc@example.com" },
            GetRoleEmails(inboxRelations, secondItem.Id, InboxRole.BCC)
        );
        Assert.AreEqual(
            "first@example.com,first-row-cc@example.com,global-bcc@example.com",
            firstItem.ToEmails
        );
        Assert.AreEqual(
            "second@example.com,global-cc@example.com,second-row-bcc@example.com",
            secondItem.ToEmails
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
        var inboxGroup = new EmailGroup
        {
            Id = 10,
            UserId = user.Id,
            Name = "Recipients",
            Type = EmailGroupType.InBox,
        };
        var outboxGroup = new EmailGroup
        {
            Id = 11,
            UserId = user.Id,
            Name = "Senders",
            Type = EmailGroupType.OutBox,
        };
        var outbox = new Outbox
        {
            Id = 21,
            UserId = user.Id,
            EmailGroupId = outboxGroup.Id,
            Email = "sender@example.com",
            IsValid = true,
            Status = OutboxStatus.Valid,
        };
        var firstInbox = CreateInbox(
            31,
            user.Id,
            organization.Id,
            inboxGroup.Id,
            "first@example.com"
        );
        var secondInbox = CreateInbox(
            32,
            user.Id,
            organization.Id,
            inboxGroup.Id,
            "second@example.com"
        );
        var sendingGroup = new SendingGroup
        {
            Id = 20,
            UserId = user.Id,
            SendBatch = true,
            Outboxes = [outbox],
            Inboxes =
            [
                new EmailAddress { Id = firstInbox.Id, Email = firstInbox.Email },
                new EmailAddress { Id = secondInbox.Id, Email = secondInbox.Email },
            ],
            CcBoxes = [new EmailAddress { Email = "global-cc@example.com" }],
            BccBoxes = [new EmailAddress { Email = "global-bcc@example.com" }],
        };
        testDatabase.Db.AddRange(
            organization,
            user,
            inboxGroup,
            outboxGroup,
            outbox,
            firstInbox,
            secondInbox,
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

    private static Inbox CreateInbox(
        long inboxId,
        long userId,
        long organizationId,
        long emailGroupId,
        string email
    ) =>
        new()
        {
            Id = inboxId,
            UserId = userId,
            OrganizationId = organizationId,
            EmailGroupId = emailGroupId,
            Email = email,
            Status = InboxStatus.Valid,
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
        IEnumerable<SendingItemInbox> inboxRelations,
        long sendingItemId,
        InboxRole inboxRole
    ) =>
        inboxRelations
            .Where(x => x.SendingItemId == sendingItemId && x.Role == inboxRole)
            .Select(x => x.ToEmail!)
            .ToArray();
}
