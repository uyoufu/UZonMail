using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.Emails;

/// <summary>
/// 验证邮箱账户读取查询可由关系型提供程序完整翻译。
/// </summary>
[TestClass]
public sealed class EmailAccountManagementServiceQueryTests
{
    private const long OwnerUserId = 10;
    private const long OrganizationId = 20;

    [TestMethod]
    public async Task GetAsync_AppliesFiltersBeforeEmailAccountDtoProjection()
    {
        await using var testDatabase = await SqliteTestDatabase.CreateAsync();
        var selectedAccount = await SeedAccountsAsync(testDatabase.Db);
        var service = new EmailAccountManagementService(testDatabase.Db, null!, null!);

        var matchingAccounts = await service.GetAsync(
            OwnerUserId,
            selectedAccount.EmailGroupId,
            "needle"
        );
        var account = await service.GetAsync(OwnerUserId, selectedAccount.Id);

        Assert.HasCount(1, matchingAccounts);
        Assert.AreEqual(selectedAccount.Id, matchingAccounts.Single().Id);
        Assert.AreEqual(selectedAccount.Id, account.Id);
        Assert.IsNotNull(account.Sender);
        Assert.IsTrue(account.Sender.HasCredential);
        Assert.IsNotNull(account.Sender.SmtpCredential);
        Assert.AreEqual("smtp.example.com", account.Sender.SmtpCredential.Host);
        Assert.AreEqual(587, account.Sender.SmtpCredential.Port);
        Assert.AreEqual(
            ConnectionSecurity.StartTLS,
            account.Sender.SmtpCredential.ConnectionSecurity
        );
        Assert.AreEqual(selectedAccount.Email, account.Sender.SmtpCredential.LoginName);
        Assert.IsNotNull(account.Receiving);
        Assert.IsTrue(account.Receiving.HasCredential);
        Assert.IsNotNull(account.Receiving.ImapCredential);
        Assert.AreEqual("imap.example.com", account.Receiving.ImapCredential.Host);
        Assert.AreEqual(993, account.Receiving.ImapCredential.Port);
        Assert.AreEqual(
            ConnectionSecurity.SSL,
            account.Receiving.ImapCredential.ConnectionSecurity
        );
        Assert.AreEqual(selectedAccount.Email, account.Receiving.ImapCredential.LoginName);
        Assert.AreEqual(OAuthApplicationSource.Custom, account.OAuthApplicationSource);
        Assert.IsTrue(account.HasOAuthAuthorization);
    }

    private static async Task<EmailAccount> SeedAccountsAsync(SqlContext db)
    {
        var owner = SendCoreTestEntityFactory.CreateUser(OwnerUserId, OrganizationId);
        var otherUser = SendCoreTestEntityFactory.CreateUser(11, OrganizationId);
        var selectedGroup = new EmailGroup
        {
            UserId = owner.Id,
            Category = EmailGroupCategory.EmailAccount,
            Name = "Selected accounts",
        };
        var otherOwnerGroup = new EmailGroup
        {
            UserId = owner.Id,
            Category = EmailGroupCategory.EmailAccount,
            Name = "Other accounts",
        };
        var otherUserGroup = new EmailGroup
        {
            UserId = otherUser.Id,
            Category = EmailGroupCategory.EmailAccount,
            Name = "Other user accounts",
        };
        db.AddRange(owner, otherUser, selectedGroup, otherOwnerGroup, otherUserGroup);
        await db.SaveChangesAsync();

        var selectedAccount = new EmailAccount
        {
            UserId = owner.Id,
            OrganizationId = OrganizationId,
            EmailGroupId = selectedGroup.Id,
            Email = "selected@example.com",
            Name = "Needle account",
        };
        db.AddRange(
            selectedAccount,
            new EmailAccount
            {
                UserId = owner.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = selectedGroup.Id,
                Email = "other@example.com",
                Name = "Other account",
            },
            new EmailAccount
            {
                UserId = owner.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = otherOwnerGroup.Id,
                Email = "needle-other-group@example.com",
                Name = "Needle other group",
            },
            new EmailAccount
            {
                UserId = otherUser.Id,
                OrganizationId = OrganizationId,
                EmailGroupId = otherUserGroup.Id,
                Email = "needle-other-user@example.com",
                Name = "Needle other user",
            }
        );
        await db.SaveChangesAsync();

        var senderAccount = new SenderAccount
        {
            EmailAccountId = selectedAccount.Id,
            Protocol = SendingProtocol.Smtp,
            AuthenticationMethod = AuthenticationMethod.Password,
            Status = SenderAccountStatus.Valid,
        };
        var receivingAccount = new ReceivingAccount
        {
            EmailAccountId = selectedAccount.Id,
            Protocol = ReceivingProtocol.Imap,
            AuthenticationMethod = AuthenticationMethod.Password,
            Status = ReceivingAccountStatus.Active,
        };
        db.AddRange(
            senderAccount,
            receivingAccount,
            new EmailAccountOAuthCredential
            {
                EmailAccountId = selectedAccount.Id,
                Provider = OAuthProvider.Microsoft,
                ApplicationSource = OAuthApplicationSource.Custom,
                EncryptedRefreshToken = "refresh-token",
                EncryptionKeyVersion = "test",
            }
        );
        await db.SaveChangesAsync();

        db.AddRange(
            new SenderAccountSmtpCredential
            {
                SenderAccountId = senderAccount.Id,
                Host = "smtp.example.com",
                Port = 587,
                ConnectionSecurity = ConnectionSecurity.StartTLS,
                LoginName = selectedAccount.Email,
                EncryptedPassword = "password",
                EncryptionKeyVersion = "test",
            },
            new ReceivingAccountImapCredential
            {
                ReceivingAccountId = receivingAccount.Id,
                Host = "imap.example.com",
                Port = 993,
                ConnectionSecurity = ConnectionSecurity.SSL,
                LoginName = selectedAccount.Email,
                EncryptedPassword = "password",
                EncryptionKeyVersion = "test",
            }
        );
        await db.SaveChangesAsync();
        return selectedAccount;
    }
}
