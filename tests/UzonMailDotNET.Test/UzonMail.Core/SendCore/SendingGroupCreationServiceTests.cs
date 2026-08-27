using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Web.Exceptions;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证创建发送组时向用户返回可操作的重复收件人提示。
/// </summary>
[TestClass]
public sealed class SendingGroupCreationServiceTests
{
    [TestMethod]
    public async Task CreateSendingGroup_DuplicateRecipientsWhenDisabled_ReportsDuplicates()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        const long userId = 100;
        const long organizationId = 10;
        var user = new User
        {
            Id = userId,
            UserId = "test-user",
            Password = "password",
            DepartmentId = organizationId,
            OrganizationId = organizationId,
        };
        testDatabase.Db.Add(user);
        testDatabase.Db.AppSettings.Add(
            new AppSetting
            {
                Type = AppSettingType.User,
                UserId = userId,
                Key = nameof(SendingSetting),
                Json = new JObject { ["allowDuplicateSending"] = false },
            }
        );
        await testDatabase.Db.SaveChangesAsync();

        using var cacheManager = new DBCacheManager(Options.Create(new DBCacheOptions()));
        await cacheManager.SetSourceAsync(
            UserInfoCache.GetSourceKey(userId),
            UserInfoSnapshot.FromUser(user)
        );
        var creationService = new SendingGroupCreationService(
            testDatabase.Db,
            null!,
            new AppSettingsManager(cacheManager),
            null!,
            null!
        );
        var sendingGroup = new global::UzonMail.DB.SQL.Core.EmailSending.SendingGroup
        {
            UserId = userId,
            Data =
            [
                new JObject { ["recipientEmail"] = "recipient@example.com" },
                new JObject { ["recipientEmail"] = "recipient@example.com" },
            ],
        };

        var exception = await Assert.ThrowsExactlyAsync<KnownException>(
            () => creationService.CreateSendingGroup(sendingGroup)
        );

        Assert.AreEqual(
            "Excel 中存在重复收件人：recipient@example.com (2 次)。当前未开启“允许重复发件”，请删除重复项或开启该设置。",
            exception.Message
        );
    }
}
