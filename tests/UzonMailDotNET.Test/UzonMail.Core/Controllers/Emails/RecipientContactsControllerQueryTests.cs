using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using UzonMail.CorePlugin.Controllers.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证收件人读取在 DTO 投影前完成所有关系型筛选。
/// </summary>
[TestClass]
public sealed class RecipientContactsControllerQueryTests
{
    private const long OwnerUserId = 10;
    private const long OrganizationId = 20;

    [TestMethod]
    public async Task GetAll_AppliesFiltersBeforeRecipientContactDtoProjection()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var selectedContact = await SeedRecipientContactsAsync(testDatabase.Db);
        var controller = CreateController(testDatabase.Db, OwnerUserId);

        var listResponse = await controller.GetAll(selectedContact.EmailGroupId, "needle");
        var detailResponse = await controller.Get(selectedContact.Id);

        Assert.IsTrue(listResponse.Ok);
        Assert.IsNotNull(listResponse.Data);
        Assert.HasCount(1, listResponse.Data);
        Assert.AreEqual(selectedContact.Id, listResponse.Data.Single().Id);
        Assert.IsTrue(detailResponse.Ok);
        Assert.IsNotNull(detailResponse.Data);
        Assert.AreEqual(selectedContact.Id, detailResponse.Data.Id);
    }

    private static async Task<RecipientContact> SeedRecipientContactsAsync(SqlContext db)
    {
        var owner = SendCoreTestEntityFactory.CreateUser(OwnerUserId, OrganizationId);
        var otherUser = SendCoreTestEntityFactory.CreateUser(11, OrganizationId);
        var selectedGroup = new EmailGroup
        {
            UserId = owner.Id,
            Category = EmailGroupCategory.RecipientEmail,
            Name = "Selected recipients",
        };
        var otherOwnerGroup = new EmailGroup
        {
            UserId = owner.Id,
            Category = EmailGroupCategory.RecipientEmail,
            Name = "Other recipients",
        };
        var otherUserGroup = new EmailGroup
        {
            UserId = otherUser.Id,
            Category = EmailGroupCategory.RecipientEmail,
            Name = "Other user recipients",
        };
        db.AddRange(owner, otherUser, selectedGroup, otherOwnerGroup, otherUserGroup);
        await db.SaveChangesAsync();

        var selectedContact = new RecipientContact
        {
            UserId = owner.Id,
            OrganizationId = OrganizationId,
            EmailGroupId = selectedGroup.Id,
            Email = "selected@example.com",
            Name = "Needle contact",
        };
        db.AddRange(
            selectedContact,
            new RecipientContact
            {
                UserId = owner.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = selectedGroup.Id,
                Email = "other@example.com",
                Name = "Other contact",
            },
            new RecipientContact
            {
                UserId = owner.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = otherOwnerGroup.Id,
                Email = "needle-other-group@example.com",
                Name = "Needle other group",
            },
            new RecipientContact
            {
                UserId = otherUser.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = otherUserGroup.Id,
                Email = "needle-other-user@example.com",
                Name = "Needle other user",
            }
        );
        await db.SaveChangesAsync();
        return selectedContact;
    }

    private static RecipientContactsController CreateController(SqlContext db, long userId)
    {
        var httpContext = new DefaultHttpContext();
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(claims: [new Claim("userId", userId.ToString())])
        );
        httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        var tokenService = new TokenService(new HttpContextAccessor { HttpContext = httpContext });
        return new RecipientContactsController(db, tokenService);
    }
}
