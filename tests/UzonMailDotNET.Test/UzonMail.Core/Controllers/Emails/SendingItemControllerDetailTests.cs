using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using UzonMail.CorePlugin.Controllers.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.DB.SQL.Core.Organization;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 验证完整邮件详情只会返回给发件项所属用户。
/// </summary>
[TestClass]
public sealed class SendingItemControllerDetailTests
{
    [TestMethod]
    public async Task GetSendingItemDetail_ReturnsCompleteOwnedEmail()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var (owner, otherUser, sendingItem) = await SeedSendingItemAsync(testDatabase.Db);
        var controller = CreateController(testDatabase.Db, owner);

        var response = await controller.GetSendingItemDetail(
            sendingItem.Id,
            CancellationToken.None
        );

        Assert.IsTrue(response.Ok);
        Assert.IsNotNull(response.Data);
        Assert.AreEqual("Quarterly update", response.Data.Subject);
        Assert.AreEqual("sender@example.com", response.Data.SenderEmail);
        Assert.AreEqual("<p>Hello customer</p>", response.Data.Content);
        Assert.AreEqual("recipient@example.com", response.Data.Recipients.Single().Email);
        Assert.AreEqual("Copy", response.Data.CcRecipients.Single().Name);
        Assert.AreEqual("hidden@example.com", response.Data.BccRecipients.Single().Email);
        Assert.AreEqual("report.pdf", response.Data.Attachments.Single().DisplayName);
        Assert.AreEqual(4096L, response.Data.Attachments.Single().Size);
        Assert.AreNotEqual(owner.Id, otherUser.Id);
    }

    [TestMethod]
    public async Task GetSendingItemDetail_DoesNotRevealForeignOrMissingEmail()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var (_, otherUser, sendingItem) = await SeedSendingItemAsync(testDatabase.Db);
        var controller = CreateController(testDatabase.Db, otherUser);

        var foreignResponse = await controller.GetSendingItemDetail(
            sendingItem.Id,
            CancellationToken.None
        );
        var missingResponse = await controller.GetSendingItemDetail(
            long.MaxValue,
            CancellationToken.None
        );

        Assert.IsFalse(foreignResponse.Ok);
        Assert.IsNull(foreignResponse.Data);
        Assert.IsFalse(missingResponse.Ok);
        Assert.IsNull(missingResponse.Data);
        Assert.AreEqual(missingResponse.Message, foreignResponse.Message);
    }

    private static async Task<(
        User Owner,
        User OtherUser,
        SendingItem SendingItem
    )> SeedSendingItemAsync(SqlContext db)
    {
        var organization = new Department
        {
            Id = 1,
            Name = "Organization",
            FullPath = "/1",
            Type = DepartmentType.Organization,
        };
        var owner = CreateUser(101, organization.Id);
        var otherUser = CreateUser(102, organization.Id);
        db.AddRange(organization, owner, otherUser);
        await db.SaveChangesAsync();

        var sendingGroup = new SendingGroup { UserId = owner.Id, Subjects = "Quarterly update" };
        var fileBucket = new FileBucket { BucketName = "attachments", RootDir = "attachments" };
        var fileCategory = new FileCategory
        {
            OwnerUserId = owner.Id,
            Name = FileCategory.DefaultName,
            IsDefault = true,
        };
        db.AddRange(sendingGroup, fileBucket, fileCategory);
        await db.SaveChangesAsync();

        var fileObject = new FileObject
        {
            FileBucketId = fileBucket.Id,
            FileBucket = fileBucket,
            Sha256 = new string('a', 64),
            Path = "report.pdf",
            Size = 4096,
        };
        db.Add(fileObject);
        await db.SaveChangesAsync();

        var attachment = new FileUsage
        {
            OwnerUserId = owner.Id,
            CategoryId = fileCategory.Id,
            Category = fileCategory,
            FileName = "report.pdf",
            DisplayName = "report.pdf",
            DisplayNameKey = "REPORT.PDF",
            FileObjectId = fileObject.Id,
            FileObject = fileObject,
        };
        db.Add(attachment);
        await db.SaveChangesAsync();

        var sendingItem = new SendingItem
        {
            SendingGroupId = sendingGroup.Id,
            SendingGroup = sendingGroup,
            UserId = owner.Id,
            OrganizationId = organization.Id,
            Subject = "Quarterly update",
            SenderEmail = "sender@example.com",
            SendDate = new DateTime(2026, 8, 11, 2, 30, 0, DateTimeKind.Utc),
            Status = SendingItemStatus.Success,
            Content = "<p>Hello customer</p>",
            Recipients = [new EmailAddress { Email = "recipient@example.com", Name = "Recipient" }],
            CC = [new EmailAddress { Email = "copy@example.com", Name = "Copy" }],
            BCC = [new EmailAddress { Email = "hidden@example.com", Name = "Hidden" }],
            Attachments = [attachment],
        };
        db.Add(sendingItem);
        await db.SaveChangesAsync();
        return (owner, otherUser, sendingItem);
    }

    private static User CreateUser(long id, long organizationId) =>
        new()
        {
            Id = id,
            UserId = $"user-{id}",
            Password = "password",
            OrganizationId = organizationId,
            DepartmentId = organizationId,
        };

    private static SendingItemController CreateController(SqlContext db, User currentUser)
    {
        var httpContext = new DefaultHttpContext();
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(claims: [new Claim("userId", currentUser.Id.ToString())])
        );
        httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {accessToken}";
        var tokenService = new TokenService(new HttpContextAccessor { HttpContext = httpContext });
        return new SendingItemController(db, tokenService);
    }
}
