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
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证收件箱无效状态在组织内同步，并能被后续导入继承
/// </summary>
[TestClass]
public sealed class EmailBoxControllerInboxStatusTests
{
    [TestMethod]
    public async Task UpdateInboxesStatus_SynchronizesOnlyMatchingOrganizationAddresses()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var organization = CreateOrganization(1);
        var otherOrganization = CreateOrganization(2);
        var currentUser = CreateUser(101, organization.Id);
        var sameOrganizationUser = CreateUser(102, organization.Id);
        var otherOrganizationUser = CreateUser(201, otherOrganization.Id);
        var currentGroup = CreateInboxGroup(1, currentUser.Id);
        var sameOrganizationGroup = CreateInboxGroup(2, sameOrganizationUser.Id);
        var otherOrganizationGroup = CreateInboxGroup(3, otherOrganizationUser.Id);
        testDatabase.Db.AddRange(
            organization,
            otherOrganization,
            currentUser,
            sameOrganizationUser,
            otherOrganizationUser,
            currentGroup,
            sameOrganizationGroup,
            otherOrganizationGroup
        );
        await testDatabase.Db.SaveChangesAsync();

        var currentInbox = CreateInbox(
            currentUser,
            currentGroup,
            "shared@example.com",
            InboxStatus.Unkown,
            "old"
        );
        var sameOrganizationInbox = CreateInbox(
            sameOrganizationUser,
            sameOrganizationGroup,
            "shared@example.com",
            InboxStatus.Valid,
            "old"
        );
        var otherOrganizationInbox = CreateInbox(
            otherOrganizationUser,
            otherOrganizationGroup,
            "shared@example.com",
            InboxStatus.Invalid,
            "other organization"
        );
        testDatabase.Db.Inboxes.AddRange(
            currentInbox,
            sameOrganizationInbox,
            otherOrganizationInbox
        );
        await testDatabase.Db.SaveChangesAsync();

        var controller = CreateController(testDatabase.Db, currentUser);
        await controller.UpdateInboxesStatus(
            new UpdateInboxesStatusDto
            {
                InboxIds = [currentInbox.Id],
                Status = InboxStatus.Invalid,
            }
        );

        testDatabase.Db.ChangeTracker.Clear();
        var inboxesAfterInvalid = await testDatabase
            .Db.Inboxes.OrderBy(x => x.UserId)
            .ToListAsync();
        Assert.IsTrue(
            inboxesAfterInvalid
                .Where(x => x.OrganizationId == organization.Id)
                .All(x => x.Status == InboxStatus.Invalid && x.ValidFailReason == null)
        );
        Assert.AreEqual(
            InboxStatus.Invalid,
            inboxesAfterInvalid.Single(x => x.OrganizationId == otherOrganization.Id).Status
        );
        Assert.AreEqual(
            "other organization",
            inboxesAfterInvalid
                .Single(x => x.OrganizationId == otherOrganization.Id)
                .ValidFailReason
        );

        await controller.UpdateInboxesStatus(
            new UpdateInboxesStatusDto { InboxIds = [currentInbox.Id], Status = InboxStatus.Valid, }
        );

        testDatabase.Db.ChangeTracker.Clear();
        var inboxesAfterValid = await testDatabase.Db.Inboxes.OrderBy(x => x.UserId).ToListAsync();
        Assert.IsTrue(
            inboxesAfterValid
                .Where(x => x.OrganizationId == organization.Id)
                .All(x => x.Status == InboxStatus.Valid && x.ValidFailReason == null)
        );
    }

    [TestMethod]
    public async Task CreateInboxes_UsesOrganizationInvalidStatusAndStoresOrganizationId()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var organization = CreateOrganization(1);
        var currentUser = CreateUser(101, organization.Id);
        var sameOrganizationUser = CreateUser(102, organization.Id);
        var currentGroup = CreateInboxGroup(1, currentUser.Id);
        var sameOrganizationGroup = CreateInboxGroup(2, sameOrganizationUser.Id);
        testDatabase.Db.AddRange(
            organization,
            currentUser,
            sameOrganizationUser,
            currentGroup,
            sameOrganizationGroup
        );
        await testDatabase.Db.SaveChangesAsync();
        testDatabase.Db.Inboxes.Add(
            CreateInbox(
                sameOrganizationUser,
                sameOrganizationGroup,
                "invalid@example.com",
                InboxStatus.Invalid,
                "validation failed"
            )
        );
        await testDatabase.Db.SaveChangesAsync();

        var controller = CreateController(testDatabase.Db, currentUser);
        await controller.CreateInboxes(
            [new CreateInboxDto { EmailGroupId = currentGroup.Id, Email = "invalid@example.com", },]
        );

        testDatabase.Db.ChangeTracker.Clear();
        var importedInbox = await testDatabase.Db.Inboxes.SingleAsync(x =>
            x.UserId == currentUser.Id
        );
        Assert.AreEqual(organization.Id, importedInbox.OrganizationId);
        Assert.AreEqual(currentGroup.Id, importedInbox.EmailGroupId);
        Assert.AreEqual(InboxStatus.Invalid, importedInbox.Status);
    }

    private static EmailBoxController CreateController(SqlContext db, User currentUser)
    {
        var httpContext = new DefaultHttpContext();
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                claims:
                [
                    new Claim("userId", currentUser.Id.ToString()),
                    new Claim("organizationId", currentUser.OrganizationId.ToString()),
                ]
            )
        );
        httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        var tokenService = new TokenService(new HttpContextAccessor { HttpContext = httpContext });
        return new EmailBoxController(
            db,
            tokenService,
            new EmailGroupService(db, tokenService),
            null!,
            null!
        );
    }

    private static Department CreateOrganization(long id) =>
        new()
        {
            Id = id,
            Name = $"Organization {id}",
            FullPath = $"/{id}",
            Type = DepartmentType.Organization,
        };

    private static User CreateUser(long id, long organizationId) =>
        new()
        {
            Id = id,
            UserId = $"user-{id}",
            Password = "password",
            OrganizationId = organizationId,
            DepartmentId = organizationId,
        };

    private static EmailGroup CreateInboxGroup(long id, long userId) =>
        new()
        {
            Id = id,
            UserId = userId,
            Name = $"Inbox group {id}",
            Type = EmailGroupType.InBox,
        };

    private static Inbox CreateInbox(
        User user,
        EmailGroup group,
        string email,
        InboxStatus status,
        string? validFailReason
    ) =>
        new()
        {
            UserId = user.Id,
            OrganizationId = user.OrganizationId,
            EmailGroupId = group.Id,
            Email = email,
            Status = status,
            ValidFailReason = validFailReason,
        };
}
