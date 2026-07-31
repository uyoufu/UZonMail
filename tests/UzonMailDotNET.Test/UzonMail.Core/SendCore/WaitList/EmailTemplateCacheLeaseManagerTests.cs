using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.DB.SQL.Core.Templates;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.WaitList;

/// <summary>
/// 验证模板内容缓存与发件组租约的刷新、授权和回收边界。
/// </summary>
[TestClass]
public sealed class EmailTemplateCacheLeaseManagerTests
{
    [TestMethod]
    public async Task TemplateCache_ReusesSnapshotAcrossGroups_AndReloadsAfterLastLease()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var template = await CreateOwnedTemplateAsync(testDatabase, "first content");
        using var cacheManager = CreateCacheManager();
        await using var leaseManager = CreateLeaseManager(cacheManager);
        await using var firstResolver = new SendingGroupTemplateResolver(
            template.UserId,
            leaseManager
        );
        await using var secondResolver = new SendingGroupTemplateResolver(
            template.UserId,
            leaseManager
        );

        var firstRead = await firstResolver.GetTemplateById(testDatabase.Db, template.Id);
        var secondRead = await secondResolver.GetTemplateById(testDatabase.Db, template.Id);

        Assert.IsNotNull(firstRead);
        Assert.AreSame(firstRead, secondRead);

        await firstResolver.DisposeAsync();
        await secondResolver.DisposeAsync();
        template.Content = "content after release";
        await testDatabase.Db.SaveChangesAsync();

        await using var nextResolver = new SendingGroupTemplateResolver(
            template.UserId,
            leaseManager
        );
        var reloadedRead = await nextResolver.GetTemplateById(testDatabase.Db, template.Id);

        Assert.AreEqual("content after release", reloadedRead?.Content);
    }

    [TestMethod]
    public async Task TemplateCache_RefreshesAfterDirty_AndRejectsUnauthorizedUser()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var owner = await CreateUserAsync(testDatabase, "owner");
        var sharedUser = await CreateUserAsync(testDatabase, "shared");
        var unrelatedUser = await CreateUserAsync(testDatabase, "unrelated");
        var template = new EmailTemplate
        {
            UserId = owner.Id,
            Name = "shared template",
            Content = "first content",
            ShareToUsers = [sharedUser],
        };
        testDatabase.Db.EmailTemplates.Add(template);
        await testDatabase.Db.SaveChangesAsync();

        using var cacheManager = CreateCacheManager();
        await using var leaseManager = CreateLeaseManager(cacheManager);
        await using var sharedResolver = new SendingGroupTemplateResolver(
            sharedUser.Id,
            leaseManager
        );
        await using var unrelatedResolver = new SendingGroupTemplateResolver(
            unrelatedUser.Id,
            leaseManager
        );

        var sharedRead = await sharedResolver.GetTemplateById(testDatabase.Db, template.Id);
        var unauthorizedRead = await unrelatedResolver.GetTemplateById(
            testDatabase.Db,
            template.Id
        );

        Assert.AreEqual("first content", sharedRead?.Content);
        Assert.IsNull(unauthorizedRead);

        template.Content = "updated content";
        await testDatabase.Db.SaveChangesAsync();
        await leaseManager.MarkDirtyAsync(template.Id);

        var refreshedRead = await sharedResolver.GetTemplateById(testDatabase.Db, template.Id);
        Assert.AreEqual("updated content", refreshedRead?.Content);

        testDatabase.Db.EmailTemplates.Remove(template);
        await testDatabase.Db.SaveChangesAsync();
        await leaseManager.MarkDirtyAsync(template.Id);

        Assert.IsNull(await sharedResolver.GetTemplateById(testDatabase.Db, template.Id));
    }

    [TestMethod]
    public async Task TemplateNameLookup_DoesNotCacheMissingResult_AndIdleLeaseReloadsContent()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var owner = await CreateUserAsync(testDatabase, "owner");
        using var cacheManager = CreateCacheManager();
        await using var leaseManager = CreateLeaseManager(cacheManager);
        await using var resolver = new SendingGroupTemplateResolver(owner.Id, leaseManager);

        Assert.IsNull(await resolver.GetTemplateByName(testDatabase.Db, "new template"));

        var template = new EmailTemplate
        {
            UserId = owner.Id,
            Name = "new template",
            Content = "initial content",
        };
        testDatabase.Db.EmailTemplates.Add(template);
        await testDatabase.Db.SaveChangesAsync();

        var firstRead = await resolver.GetTemplateByName(testDatabase.Db, template.Name);
        Assert.AreEqual("initial content", firstRead?.Content);

        template.Content = "reloaded after idle";
        await testDatabase.Db.SaveChangesAsync();
        await leaseManager.CleanupIdleCachesAsync(DateTimeOffset.UtcNow.AddMinutes(31));

        var reloadedRead = await resolver.GetTemplateByName(testDatabase.Db, template.Name);
        Assert.AreEqual("reloaded after idle", reloadedRead?.Content);
    }

    private static DBCacheManager CreateCacheManager()
    {
        return new DBCacheManager(
            Options.Create(
                new DBCacheOptions
                {
                    SlidingExpirationMinutes = 30,
                    ExpirationScanFrequencyMinutes = 5,
                }
            )
        );
    }

    private static EmailTemplateCacheLeaseManager CreateLeaseManager(DBCacheManager cacheManager)
    {
        return new EmailTemplateCacheLeaseManager(
            cacheManager,
            Options.Create(
                new DBCacheOptions
                {
                    SlidingExpirationMinutes = 30,
                    ExpirationScanFrequencyMinutes = 5,
                }
            ),
            TimeProvider.System
        );
    }

    private static async Task<EmailTemplate> CreateOwnedTemplateAsync(
        SqliteTestDatabase testDatabase,
        string content
    )
    {
        var owner = await CreateUserAsync(testDatabase, $"owner-{Guid.NewGuid():N}");
        var template = new EmailTemplate
        {
            UserId = owner.Id,
            Name = $"template-{Guid.NewGuid():N}",
            Content = content,
        };
        testDatabase.Db.EmailTemplates.Add(template);
        await testDatabase.Db.SaveChangesAsync();
        return template;
    }

    private static async Task<User> CreateUserAsync(SqliteTestDatabase testDatabase, string userId)
    {
        var user = new User
        {
            UserId = $"{userId}-{Guid.NewGuid():N}",
            Password = "test-password",
            OrganizationId = 1,
            DepartmentId = 1,
        };
        testDatabase.Db.Users.Add(user);
        await testDatabase.Db.SaveChangesAsync();
        return user;
    }
}
