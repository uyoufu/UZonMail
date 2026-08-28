using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.DB.SQL.Core.Emails;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Emails;

/// <summary>
/// 验证 IMAP 信息库查询和未知域回退规则。
/// </summary>
[TestClass]
public sealed class ImapInfoServiceTests
{
    [TestMethod]
    public async Task GuessImapInfos_UsesKnownDomainAndNormalizesEmailDomain()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        testDatabase.Db.ImapInfos.Add(
            new ImapInfo
            {
                Domain = "example.com",
                Host = "mail.example.com",
                Port = 143,
                ConnectionSecurity = ConnectionSecurity.StartTLS,
            }
        );
        await testDatabase.Db.SaveChangesAsync();
        var service = new ImapInfoService(testDatabase.Db);

        var results = await service.GuessImapInfos(["Owner@EXAMPLE.COM"]);

        var result = results["Owner@EXAMPLE.COM"];
        Assert.AreEqual("mail.example.com", result.Host);
        Assert.AreEqual(143, result.Port);
        Assert.AreEqual(ConnectionSecurity.StartTLS, result.ConnectionSecurity);
    }

    [TestMethod]
    public async Task GuessImapInfos_FallsBackForUnknownDomainAndSkipsInvalidEmails()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var service = new ImapInfoService(testDatabase.Db);

        var results = await service.GuessImapInfos(["owner@unknown.test", "invalid-email"]);

        Assert.HasCount(1, results);
        var result = results["owner@unknown.test"];
        Assert.AreEqual("unknown.test", result.Domain);
        Assert.AreEqual("imap.unknown.test", result.Host);
        Assert.AreEqual(993, result.Port);
        Assert.AreEqual(ConnectionSecurity.SSL, result.ConnectionSecurity);
    }

    [TestMethod]
    public async Task ImapInfoModel_RequiresUniqueDomain()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();

        var entityType = testDatabase.Db.Model.FindEntityType(typeof(ImapInfo));
        var domainIndex = entityType
            ?.GetIndexes()
            .Single(index =>
                index
                    .Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(ImapInfo.Domain)])
            );

        Assert.IsNotNull(domainIndex);
        Assert.IsTrue(domainIndex.IsUnique);
    }
}
