using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMailDotNET.Test.UzonMail.DB.EmailReceiving;

/// <summary>
/// 验证 IMAP 收件领域的 EF Core 模型、约束和查询过滤器。
/// </summary>
[TestClass]
public sealed class EmailReceivingModelTests
{
    [TestMethod]
    public async Task EnsureCreatedAsync_RegistersCompleteReceivingModel()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);

        await db.Database.EnsureCreatedAsync();

        Assert.IsNotNull(db.Model.FindEntityType(typeof(ImapAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ImapMailbox)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ImapSyncCommand)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailMessage)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ImapMailboxSyncCheckpoint)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailMimePart)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailAnalysis)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailDeliveryStatus)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailFeedbackReport)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailClassificationEvidence)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(IncomingMailAuditEvent)));

        var locationType = db.Model.FindEntityType(typeof(IncomingMailLocation))!;
        var locationMailboxForeignKey = locationType
            .GetForeignKeys()
            .Single(x => x.PrincipalEntityType.ClrType == typeof(ImapMailbox));
        Assert.AreEqual(DeleteBehavior.NoAction, locationMailboxForeignKey.DeleteBehavior);
    }

    [TestMethod]
    public async Task ImapAccountIndex_EnforcesActiveAccountUniqueness()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var account = new ImapAccount
        {
            UserId = 11,
            OrganizationId = 22,
            Name = "Primary",
            Email = " Inbox@Example.COM ",
            Host = "imap.example.com",
        };
        db.ImapAccounts.Add(account);
        await db.SaveChangesAsync();
        Assert.AreEqual("inbox@example.com", account.Email);

        db.ImapAccounts.Add(
            new ImapAccount
            {
                UserId = account.UserId,
                OrganizationId = account.OrganizationId,
                Name = "Duplicate",
                Email = account.Email,
                Host = account.Host,
                Port = account.Port,
            }
        );
        await Assert.ThrowsExactlyAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task IncomingMailLocationIndex_RejectsDuplicateRemoteUid()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var account = new ImapAccount
        {
            UserId = 11,
            OrganizationId = 22,
            Name = "Primary",
            Email = "inbox@example.com",
            Host = "imap.example.com",
        };
        db.ImapAccounts.Add(account);
        await db.SaveChangesAsync();

        var mailbox = new ImapMailbox
        {
            ImapAccountId = account.Id,
            RemoteFullName = "INBOX",
            DisplayName = "INBOX",
            UidValidity = 100,
        };
        db.ImapMailboxes.Add(mailbox);
        await db.SaveChangesAsync();

        var firstMessage = new IncomingMailMessage
        {
            ImapAccountId = account.Id,
            ReceivedAtUtc = DateTime.UtcNow,
        };
        var duplicateMessage = new IncomingMailMessage
        {
            ImapAccountId = account.Id,
            ReceivedAtUtc = DateTime.UtcNow,
        };
        db.IncomingMailMessages.AddRange(firstMessage, duplicateMessage);
        await db.SaveChangesAsync();

        db.IncomingMailLocations.AddRange(
            new IncomingMailLocation
            {
                ImapAccountId = account.Id,
                IncomingMailMessageId = firstMessage.Id,
                ImapMailboxId = mailbox.Id,
                UidValidity = mailbox.UidValidity!.Value,
                Uid = 101,
                FirstSeenAtUtc = DateTime.UtcNow,
                LastSynchronizedAtUtc = DateTime.UtcNow,
            },
            new IncomingMailLocation
            {
                ImapAccountId = account.Id,
                IncomingMailMessageId = duplicateMessage.Id,
                ImapMailboxId = mailbox.Id,
                UidValidity = mailbox.UidValidity!.Value,
                Uid = 101,
                FirstSeenAtUtc = DateTime.UtcNow,
                LastSynchronizedAtUtc = DateTime.UtcNow,
            }
        );

        await Assert.ThrowsExactlyAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ReceivingModel_UsesNormalizedInternetMessageIdIndex()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var sendingItemType = db.Model.FindEntityType(typeof(SendingItem))!;
        var internetMessageIdIndex = sendingItemType
            .GetIndexes()
            .Single(x =>
                x.Properties.Select(property => property.Name)
                    .SequenceEqual(["InternetMessageIdKey"])
            );
        Assert.IsTrue(internetMessageIdIndex.IsUnique);
    }

    [TestMethod]
    public async Task IncomingMailMimePart_PreservesUndownloadedAttachmentMetadata()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var account = CreateAccount();
        db.ImapAccounts.Add(account);
        await db.SaveChangesAsync();

        var message = new IncomingMailMessage
        {
            ImapAccountId = account.Id,
            ReceivedAtUtc = DateTime.UtcNow,
            AttachmentCount = 1,
        };
        db.IncomingMailMessages.Add(message);
        await db.SaveChangesAsync();

        var mimePart = new IncomingMailMimePart
        {
            IncomingMailMessageId = message.Id,
            MimePartPath = "2.1",
            PartKind = IncomingMailMimePartKind.Attachment,
            ContentDisposition = IncomingMailContentDisposition.Attachment,
            FileName = "report.pdf",
            MediaType = "application/pdf",
            DeclaredSize = 2048,
            FetchStatus = IncomingMailMimePartFetchStatus.NotDownloaded,
        };
        db.IncomingMailMimeParts.Add(mimePart);
        await db.SaveChangesAsync();

        Assert.IsNull(mimePart.FileUsageId);
        Assert.AreEqual(IncomingMailMimePartFetchStatus.NotDownloaded, mimePart.FetchStatus);
    }

    [TestMethod]
    public async Task ImapMailboxSyncCheckpoint_RejectsMultipleCheckpointsForOneMailbox()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var account = CreateAccount();
        db.ImapAccounts.Add(account);
        await db.SaveChangesAsync();
        var mailbox = new ImapMailbox
        {
            ImapAccountId = account.Id,
            RemoteFullName = "INBOX",
            DisplayName = "INBOX",
        };
        db.ImapMailboxes.Add(mailbox);
        await db.SaveChangesAsync();

        db.ImapMailboxSyncCheckpoints.Add(
            new ImapMailboxSyncCheckpoint { ImapMailboxId = mailbox.Id, UidValidity = 1 }
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.ImapMailboxSyncCheckpoints.Add(
            new ImapMailboxSyncCheckpoint { ImapMailboxId = mailbox.Id, UidValidity = 2 }
        );

        await Assert.ThrowsExactlyAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ReceivingModel_UsesAccountScopedLocationForeignKeys()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var locationType = db.Model.FindEntityType(typeof(IncomingMailLocation))!;
        Assert.IsTrue(
            locationType
                .GetForeignKeys()
                .Any(x =>
                    x.Properties.Select(property => property.Name)
                        .SequenceEqual(["IncomingMailMessageId", "ImapAccountId"])
                )
        );
        Assert.IsTrue(
            locationType
                .GetForeignKeys()
                .Any(x =>
                    x.Properties.Select(property => property.Name)
                        .SequenceEqual(["ImapMailboxId", "ImapAccountId"])
                )
        );
    }

    [TestMethod]
    public async Task SoftDeletedAccount_IsExcludedFromReceivingQueries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.ImapAccounts.Add(
            new ImapAccount
            {
                UserId = 11,
                OrganizationId = 22,
                Name = "Archived",
                Email = "archived@example.com",
                Host = "imap.example.com",
                IsDeleted = true,
            }
        );
        await db.SaveChangesAsync();

        Assert.AreEqual(0, await db.ImapAccounts.CountAsync());
        Assert.AreEqual(1, await db.ImapAccounts.IgnoreQueryFilters().CountAsync());
    }

    private static SqlContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SqlContext>().UseSqlite(connection).Options;
        return new SqlContext(options);
    }

    private static ImapAccount CreateAccount() =>
        new()
        {
            UserId = 11,
            OrganizationId = 22,
            Name = "Primary",
            Email = "inbox@example.com",
            Host = "imap.example.com",
        };
}
