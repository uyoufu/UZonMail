using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using UzonMail.CorePlugin.Controllers.Emails;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.Utils.Web.Exceptions;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证邮箱批量移动仅影响当前用户且目标分组类型正确
/// </summary>
[TestClass]
public sealed class EmailBoxControllerMailboxMoveTests
{
    [TestMethod]
    public async Task MoveEmailBoxesToGroup_UpdatesOnlyCurrentUsersInboxesAndOutboxes()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var (controller, organization, currentUser, otherUser) = CreateController(testDatabase.Db);
        var inboxSourceGroup = CreateGroup(1, currentUser.Id, EmailGroupType.InBox);
        var inboxTargetGroup = CreateGroup(2, currentUser.Id, EmailGroupType.InBox);
        var outboxSourceGroup = CreateGroup(3, currentUser.Id, EmailGroupType.OutBox);
        var outboxTargetGroup = CreateGroup(4, currentUser.Id, EmailGroupType.OutBox);
        testDatabase.Db.AddRange(
            organization,
            currentUser,
            otherUser,
            inboxSourceGroup,
            inboxTargetGroup,
            outboxSourceGroup,
            outboxTargetGroup
        );
        await testDatabase.Db.SaveChangesAsync();

        var currentUserInbox = new Inbox
        {
            UserId = currentUser.Id,
            OrganizationId = organization.Id,
            EmailGroupId = inboxSourceGroup.Id,
            Email = "current-inbox@example.com"
        };
        var otherUserInbox = new Inbox
        {
            UserId = otherUser.Id,
            OrganizationId = organization.Id,
            EmailGroupId = inboxSourceGroup.Id,
            Email = "other-inbox@example.com"
        };
        var currentUserOutbox = new Outbox
        {
            UserId = currentUser.Id,
            EmailGroupId = outboxSourceGroup.Id,
            Email = "current-outbox@example.com"
        };
        var otherUserOutbox = new Outbox
        {
            UserId = otherUser.Id,
            EmailGroupId = outboxSourceGroup.Id,
            Email = "other-outbox@example.com"
        };
        testDatabase.Db.AddRange(
            currentUserInbox,
            otherUserInbox,
            currentUserOutbox,
            otherUserOutbox
        );
        await testDatabase.Db.SaveChangesAsync();

        await controller.MoveInboxesToGroup(
            new MoveEmailBoxesDto
            {
                EmailBoxIds = [currentUserInbox.Id, otherUserInbox.Id],
                TargetGroupId = inboxTargetGroup.Id
            }
        );
        await controller.MoveOutboxesToGroup(
            new MoveEmailBoxesDto
            {
                EmailBoxIds = [currentUserOutbox.Id, otherUserOutbox.Id],
                TargetGroupId = outboxTargetGroup.Id
            }
        );

        testDatabase.Db.ChangeTracker.Clear();
        Assert.AreEqual(
            inboxTargetGroup.Id,
            await testDatabase
                .Db.Inboxes.Where(inbox => inbox.Id == currentUserInbox.Id)
                .Select(inbox => inbox.EmailGroupId)
                .SingleAsync()
        );
        Assert.AreEqual(
            inboxSourceGroup.Id,
            await testDatabase
                .Db.Inboxes.Where(inbox => inbox.Id == otherUserInbox.Id)
                .Select(inbox => inbox.EmailGroupId)
                .SingleAsync()
        );
        Assert.AreEqual(
            outboxTargetGroup.Id,
            await testDatabase
                .Db.Outboxes.Where(outbox => outbox.Id == currentUserOutbox.Id)
                .Select(outbox => outbox.EmailGroupId)
                .SingleAsync()
        );
        Assert.AreEqual(
            outboxSourceGroup.Id,
            await testDatabase
                .Db.Outboxes.Where(outbox => outbox.Id == otherUserOutbox.Id)
                .Select(outbox => outbox.EmailGroupId)
                .SingleAsync()
        );
    }

    [TestMethod]
    public async Task MoveInboxesToGroup_RejectsOutboxTargetGroup()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var (controller, organization, currentUser, _) = CreateController(testDatabase.Db);
        var inboxSourceGroup = CreateGroup(1, currentUser.Id, EmailGroupType.InBox);
        var outboxTargetGroup = CreateGroup(2, currentUser.Id, EmailGroupType.OutBox);
        testDatabase.Db.AddRange(organization, currentUser, inboxSourceGroup, outboxTargetGroup);
        var inbox = new Inbox
        {
            UserId = currentUser.Id,
            OrganizationId = organization.Id,
            EmailGroupId = inboxSourceGroup.Id,
            Email = "inbox@example.com"
        };
        testDatabase.Db.Inboxes.Add(inbox);
        await testDatabase.Db.SaveChangesAsync();

        var hasRejectedTargetGroup = false;
        try
        {
            await controller.MoveInboxesToGroup(
                new MoveEmailBoxesDto
                {
                    EmailBoxIds = [inbox.Id],
                    TargetGroupId = outboxTargetGroup.Id
                }
            );
        }
        catch (KnownException)
        {
            hasRejectedTargetGroup = true;
        }

        Assert.IsTrue(hasRejectedTargetGroup);

        testDatabase.Db.ChangeTracker.Clear();
        Assert.AreEqual(
            inboxSourceGroup.Id,
            await testDatabase
                .Db.Inboxes.Where(value => value.Id == inbox.Id)
                .Select(value => value.EmailGroupId)
                .SingleAsync()
        );
    }

    private static (
        EmailBoxController Controller,
        Department Organization,
        User CurrentUser,
        User OtherUser
    ) CreateController(SqlContext db)
    {
        var organization = new Department
        {
            Id = 1,
            Name = "Test organization",
            FullPath = "/1",
            Type = DepartmentType.Organization
        };
        var currentUser = new User
        {
            Id = 101,
            UserId = "current-user",
            Password = "password",
            OrganizationId = organization.Id,
            DepartmentId = organization.Id
        };
        var otherUser = new User
        {
            Id = 202,
            UserId = "other-user",
            Password = "password",
            OrganizationId = organization.Id,
            DepartmentId = organization.Id
        };
        var httpContext = new DefaultHttpContext();
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(claims: [new Claim("userId", currentUser.Id.ToString())])
        );
        httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        var tokenService = new TokenService(new HttpContextAccessor { HttpContext = httpContext });
        var controller = new EmailBoxController(
            db,
            tokenService,
            new EmailGroupService(db, tokenService),
            null!,
            null!
        );
        return (controller, organization, currentUser, otherUser);
    }

    private static EmailGroup CreateGroup(long id, long userId, EmailGroupType type) =>
        new()
        {
            Id = id,
            UserId = userId,
            Name = $"{type} group {id}",
            Type = type
        };
}
