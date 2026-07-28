using MailKit.Net.Smtp;
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

    /// <summary>
    /// SPF 在 MAIL FROM 阶段被拒绝时不能推断收件人不存在
    /// </summary>
    [TestMethod]
    public void SmtpProbe_MailFromSpfFailure_IsUnknown()
    {
        var evidence = SmtpVerificationEvidenceClassifier.Classify(
            new SmtpProbeResult(
                SmtpProbeStage.MailFrom,
                new SmtpResponse(
                    SmtpStatusCode.MailboxUnavailable,
                    "SPF check failed. IP: 111.172.50.184"
                )
            )
        );

        Assert.AreEqual(InboxVerificationState.Unknown, evidence.State);
        Assert.IsTrue(evidence.CanConnectSmtp);
        Assert.IsFalse(evidence.IsDisabled);
    }

    /// <summary>
    /// 只有收件人阶段的永久拒绝才标记地址无效
    /// </summary>
    [TestMethod]
    public void SmtpProbe_RecipientPermanentRejection_IsInvalid()
    {
        var evidence = SmtpVerificationEvidenceClassifier.Classify(
            new SmtpProbeResult(
                SmtpProbeStage.Recipient,
                new SmtpResponse(SmtpStatusCode.MailboxUnavailable, "邮箱不存在")
            )
        );

        Assert.AreEqual(InboxVerificationState.Invalid, evidence.State);
        Assert.IsTrue(evidence.IsDisabled);
    }

    /// <summary>
    /// SPF 误判产生的旧快照应立即重新验证
    /// </summary>
    [TestMethod]
    public void SnapshotPolicy_DoesNotReuseLegacySpfFailure()
    {
        var snapshot = new InboxVerificationSnapshot
        {
            State = InboxVerificationState.Invalid,
            FailureReason = "SPF check failed. IP: 111.172.50.184",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
        };

        Assert.IsFalse(InboxVerificationSnapshotPolicy.CanReuse(snapshot, DateTime.UtcNow));
    }

    /// <summary>
    /// 未知状态必须使用短期快照，避免策略拦截长期固化
    /// </summary>
    [TestMethod]
    public void SnapshotPolicy_UnknownStateUsesShortLifetime()
    {
        var utcNow = DateTime.UtcNow;
        var options = new InboxVerificationOptions
        {
            UnknownSnapshotLifetime = TimeSpan.FromHours(12),
            LongLivedDomainSnapshotLifetime = TimeSpan.FromDays(365),
        };

        var expiresAtUtc = InboxVerificationSnapshotPolicy.GetExpiresAtUtc(
            "qq.com",
            InboxVerificationState.Unknown,
            options,
            utcNow
        );

        Assert.AreEqual(utcNow.AddHours(12), expiresAtUtc);
    }
}
