using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.EmailVerification;
using UzonMail.ProPlugin.Services.EmailVerify;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.EmailVerify;

namespace UzonMailDotNET.Test.UzonMail.Pro.EmailVerify;

/// <summary>
/// 验证 Pro 收件箱验证快照的跨用户复用规则
/// </summary>
[TestClass]
public sealed class ProInboxVerificationEvidenceTests
{
    /// <summary>
    /// 已存在的有效快照应由不同用户共同复用
    /// </summary>
    [TestMethod]
    public async Task VerifyAsync_UsesOneFreshSnapshotForDifferentUsers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SqlContextPro>().UseSqlite(connection).Options;
        await using var db = new SqlContextPro(options);
        await db.Database.EnsureCreatedAsync();

        db.InboxVerificationSnapshots.Add(
            new InboxVerificationSnapshot
            {
                NormalizedEmail = "shared@example.com",
                State = InboxVerificationState.Valid,
                VerifiedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
                SyntaxDomain = "example.com",
                SyntaxUsername = "shared",
                IsValidSyntax = true,
                IsDeliverable = true,
            }
        );
        await db.SaveChangesAsync();

        var verifier = new ProInboxVerificationEvidence(
            db,
            Options.Create(new InboxVerificationOptions())
        );

        var firstReport = await verifier.VerifyAsync(
            new InboxVerificationRequest(1, 101, "Shared@Example.com")
        );
        var secondReport = await verifier.VerifyAsync(
            new InboxVerificationRequest(2, 202, "shared@example.com")
        );

        Assert.AreEqual(InboxVerificationState.Valid, firstReport.State);
        Assert.AreEqual(InboxVerificationState.Valid, secondReport.State);
        Assert.AreEqual(1, await db.InboxVerificationSnapshots.CountAsync());
        Assert.AreEqual(2L, secondReport.Request.UserId);
    }

    /// <summary>
    /// 快照模型仅以规范化邮箱作为唯一键
    /// </summary>
    [TestMethod]
    public void SnapshotModel_UsesNormalizedEmailAsTheOnlyUniqueKey()
    {
        var options = new DbContextOptionsBuilder<SqlContextPro>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new SqlContextPro(options);

        var entityType = db.Model.FindEntityType(typeof(InboxVerificationSnapshot));
        Assert.IsNotNull(entityType);
        Assert.IsNull(entityType.FindProperty("UserId"));

        var uniqueIndex = entityType.GetIndexes().Single(x => x.IsUnique);
        CollectionAssert.AreEqual(
            new[] { nameof(InboxVerificationSnapshot.NormalizedEmail) },
            uniqueIndex.Properties.Select(x => x.Name).ToArray()
        );
    }
}
