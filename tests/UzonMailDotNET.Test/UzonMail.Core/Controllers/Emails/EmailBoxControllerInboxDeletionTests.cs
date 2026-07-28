using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using UzonMail.CorePlugin.Controllers.Emails;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Organization;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证收件箱批量删除与单条删除保持相同的数据保留语义。
/// </summary>
[TestClass]
public sealed class EmailBoxControllerInboxDeletionTests
{
    [TestMethod]
    public async Task DeleteInboxByIds_SoftDeletesOnlyCurrentUsersInboxes()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var organization = new Department
        {
            Id = 1,
            Name = "Test organization",
            FullPath = "/1",
            Type = DepartmentType.Organization,
        };
        var currentUser = new User
        {
            Id = 101,
            UserId = "current-user",
            Password = "password",
            OrganizationId = organization.Id,
            DepartmentId = organization.Id,
        };
        var otherUser = new User
        {
            Id = 202,
            UserId = "other-user",
            Password = "password",
            OrganizationId = organization.Id,
            DepartmentId = organization.Id,
        };
        var inboxGroup = new EmailGroup
        {
            Id = 1,
            UserId = currentUser.Id,
            Name = "Inbox group",
            Type = EmailGroupType.InBox,
        };
        testDatabase.Db.AddRange(organization, currentUser, otherUser, inboxGroup);
        await testDatabase.Db.SaveChangesAsync();

        var currentUserInbox = new Inbox
        {
            UserId = 101,
            OrganizationId = organization.Id,
            EmailGroupId = inboxGroup.Id,
            Email = "current@example.com",
            ObjectId = "current-inbox",
        };
        var otherUserInbox = new Inbox
        {
            UserId = 202,
            OrganizationId = organization.Id,
            EmailGroupId = inboxGroup.Id,
            Email = "other@example.com",
            ObjectId = "other-inbox",
        };
        testDatabase.Db.Inboxes.AddRange(currentUserInbox, otherUserInbox);
        await testDatabase.Db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(claims: [new Claim("userId", "101")])
        );
        httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        var tokenService = new TokenService(new HttpContextAccessor { HttpContext = httpContext });
        var controller = new EmailBoxController(
            testDatabase.Db,
            tokenService,
            new EmailGroupService(testDatabase.Db, tokenService),
            null!,
            null!
        );

        await controller.DeleteInboxByIds([currentUserInbox.ObjectId, otherUserInbox.ObjectId]);

        testDatabase.Db.ChangeTracker.Clear();
        var inboxes = await testDatabase
            .Db.Inboxes.IgnoreQueryFilters()
            .OrderBy(inbox => inbox.UserId)
            .ToListAsync();
        Assert.IsTrue(inboxes.Single(inbox => inbox.UserId == 101).IsDeleted);
        Assert.IsFalse(inboxes.Single(inbox => inbox.UserId == 202).IsDeleted);
    }
}
