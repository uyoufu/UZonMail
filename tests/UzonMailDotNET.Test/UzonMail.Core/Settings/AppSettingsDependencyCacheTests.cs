using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.CorePlugin.Utils.Cache;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMailDotNET.Test.UzonMail.Core.Settings;

/// <summary>
/// 验证设置继承链会随共享原始依赖自动刷新。
/// </summary>
[TestClass]
public sealed class AppSettingsDependencyCacheTests
{
    [TestMethod]
    public async Task OrganizationAndUserSourceChanges_RefreshAllDependentSettingTypes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var dbOptions = new DbContextOptionsBuilder<SqlContext>().UseSqlite(connection).Options;
        await using var db = new SqlContext(dbOptions);
        await db.Database.EnsureCreatedAsync();
        const long userId = 100;
        const long firstOrganizationId = 10;
        const long secondOrganizationId = 20;
        var settingKey = nameof(TestInheritedSetting);
        db.AppSettings.AddRange(
            CreateSetting(AppSettingType.System, 0, settingKey, "system"),
            CreateSetting(
                AppSettingType.Organization,
                firstOrganizationId,
                settingKey,
                "organization-1"
            ),
            CreateSetting(
                AppSettingType.Organization,
                secondOrganizationId,
                settingKey,
                "organization-2"
            )
        );
        await db.SaveChangesAsync();

        using var cacheManager = new DBCacheManager(Options.Create(new DBCacheOptions()));
        var settingsManager = new AppSettingsManager(cacheManager);
        await cacheManager.SetSourceAsync(
            UserInfoCache.GetSourceKey(userId),
            new UserInfoSnapshot(userId, 1, firstOrganizationId)
        );

        var first = await settingsManager.GetSetting<TestInheritedSetting>(db, userId);
        var alternateFirst = await settingsManager.GetSetting<AlternateInheritedSetting>(
            db,
            userId
        );
        Assert.AreEqual("organization-1", first.Value);
        Assert.AreEqual("organization-1", alternateFirst.Value);

        var firstOrganizationKey = new AppSettingCacheKey(
            AppSettingType.Organization,
            firstOrganizationId,
            settingKey
        );
        await cacheManager.SetSourceAsync(
            AppSettingSnapshot.GetSourceKey(firstOrganizationKey),
            new AppSettingSnapshot(2, CreateJson("organization-1-updated"))
        );

        var updated = await settingsManager.GetSetting<TestInheritedSetting>(db, userId);
        var alternateUpdated = await settingsManager.GetSetting<AlternateInheritedSetting>(
            db,
            userId
        );
        Assert.AreEqual("organization-1-updated", updated.Value);
        Assert.AreEqual("organization-1-updated", alternateUpdated.Value);
        Assert.AreNotSame(first, updated);
        Assert.AreNotSame(alternateFirst, alternateUpdated);
        Assert.AreEqual("organization-1", first.Value);

        await cacheManager.SetSourceAsync(
            UserInfoCache.GetSourceKey(userId),
            new UserInfoSnapshot(userId, 1, secondOrganizationId)
        );
        var movedUserSetting = await settingsManager.GetSetting<TestInheritedSetting>(db, userId);

        Assert.AreEqual("organization-2", movedUserSetting.Value);
        Assert.AreNotSame(updated, movedUserSetting);
    }

    private static AppSetting CreateSetting(
        AppSettingType settingType,
        long ownerId,
        string settingKey,
        string value
    ) =>
        new()
        {
            Type = settingType,
            UserId = settingType == AppSettingType.User ? ownerId : 0,
            OrganizationId = settingType == AppSettingType.Organization ? ownerId : 0,
            Key = settingKey,
            Json = CreateJson(value)
        };

    private static JObject CreateJson(string value) => new() { ["value"] = value };

    private class TestInheritedSetting : BaseSettingModel
    {
        public string Value { get; private set; } = string.Empty;

        protected override void ReadValuesFromJson()
        {
            Value = GetStringValue(nameof(Value));
        }
    }

    [EntityCacheKey(nameof(TestInheritedSetting))]
    private sealed class AlternateInheritedSetting : TestInheritedSetting;
}
