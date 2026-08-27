using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;

namespace UzonMailDotNET.Test.UzonMail.DB.Emails;

[TestClass]
public sealed class EmailAccountModelTests
{
    [TestMethod]
    public async Task Model_RegistersSeparatedIdentityCapabilitiesAndCredentials()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        Assert.IsNotNull(db.Model.FindEntityType(typeof(EmailAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(SenderAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ReceivingAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(RecipientContact)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(SenderAccountSmtpCredential)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ReceivingAccountImapCredential)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(EmailAccountOAuthCredential)));

        AssertUniqueIndex<SenderAccount>(db, nameof(SenderAccount.EmailAccountId));
        AssertUniqueIndex<ReceivingAccount>(db, nameof(ReceivingAccount.EmailAccountId));
        AssertUniqueIndex<SenderAccountSmtpCredential>(
            db,
            nameof(SenderAccountSmtpCredential.SenderAccountId)
        );
        AssertUniqueIndex<ReceivingAccountImapCredential>(
            db,
            nameof(ReceivingAccountImapCredential.ReceivingAccountId)
        );
        AssertUniqueIndex<EmailAccountOAuthCredential>(
            db,
            nameof(EmailAccountOAuthCredential.EmailAccountId)
        );
    }

    [TestMethod]
    public async Task EmailAccount_EnforcesNormalizedAddressUniquenessPerUser()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.EmailAccounts.Add(
            new EmailAccount
            {
                UserId = 10,
                OrganizationId = 20,
                Email = " Sender@Example.com ",
            }
        );
        await db.SaveChangesAsync();
        db.EmailAccounts.Add(
            new EmailAccount
            {
                UserId = 10,
                OrganizationId = 20,
                Email = "sender@example.com",
            }
        );

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ReceivingSenderLinks_EnforcePairAndPrimaryUniqueness()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var emailAccount = new EmailAccount
        {
            UserId = 10,
            OrganizationId = 20,
            Email = "shared@example.com",
        };
        var receiving = new ReceivingAccount
        {
            EmailAccount = emailAccount,
            Protocol = ReceivingProtocol.Imap,
            AuthenticationMethod = AuthenticationMethod.Password,
        };
        var sender = new SenderAccount
        {
            EmailAccount = new EmailAccount
            {
                UserId = 10,
                OrganizationId = 20,
                Email = "sender@example.com",
            },
            EmailGroupId = 1,
            Protocol = SendingProtocol.Smtp,
            AuthenticationMethod = AuthenticationMethod.Password,
        };
        db.AddRange(receiving, sender);
        await db.SaveChangesAsync();

        var link = new ReceivingAccountSenderLink
        {
            ReceivingAccountId = receiving.Id,
            SenderAccountId = sender.Id,
        };
        db.ReceivingAccountSenderLinks.Add(link);
        await db.SaveChangesAsync();
        db.ReceivingAccountSenderLinks.Add(
            new ReceivingAccountSenderLink
            {
                ReceivingAccountId = receiving.Id,
                SenderAccountId = sender.Id,
            }
        );
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ReceivingPrimarySender_EnforcesOnePrimaryPerReceivingAccount()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var receivingAccount = new ReceivingAccount
        {
            EmailAccount = CreateEmailAccount("receiving@example.com"),
            Protocol = ReceivingProtocol.Imap,
            AuthenticationMethod = AuthenticationMethod.Password,
        };
        var firstSenderAccount = CreateSenderAccount("first@example.com");
        var secondSenderAccount = CreateSenderAccount("second@example.com");
        db.AddRange(receivingAccount, firstSenderAccount, secondSenderAccount);
        await db.SaveChangesAsync();

        var firstLink = new ReceivingAccountSenderLink
        {
            ReceivingAccountId = receivingAccount.Id,
            SenderAccountId = firstSenderAccount.Id,
        };
        var secondLink = new ReceivingAccountSenderLink
        {
            ReceivingAccountId = receivingAccount.Id,
            SenderAccountId = secondSenderAccount.Id,
        };
        db.AddRange(firstLink, secondLink);
        await db.SaveChangesAsync();

        db.ReceivingAccountPrimarySenders.Add(
            new ReceivingAccountPrimarySender
            {
                ReceivingAccountId = receivingAccount.Id,
                ReceivingAccountSenderLinkId = firstLink.Id,
            }
        );
        await db.SaveChangesAsync();

        var receivingAccountId = receivingAccount.Id;
        var secondLinkId = secondLink.Id;
        db.ChangeTracker.Clear();
        db.ReceivingAccountPrimarySenders.Add(
            new ReceivingAccountPrimarySender
            {
                ReceivingAccountId = receivingAccountId,
                ReceivingAccountSenderLinkId = secondLinkId,
            }
        );

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ProtocolAuthenticationConstraints_AcceptSupportedAndRejectInvalidCombinations()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.AddRange(
            CreateSenderAccount("smtp@example.com"),
            new SenderAccount
            {
                EmailAccount = CreateEmailAccount("graph-sender@example.com"),
                EmailGroupId = 1,
                Protocol = SendingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.OAuth2,
            },
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("imap@example.com"),
                Protocol = ReceivingProtocol.Imap,
                AuthenticationMethod = AuthenticationMethod.Password,
            },
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("graph-receiving@example.com"),
                Protocol = ReceivingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.OAuth2,
            }
        );
        await db.SaveChangesAsync();

        db.SenderAccounts.Add(
            new SenderAccount
            {
                EmailAccount = CreateEmailAccount("invalid-sender@example.com"),
                EmailGroupId = 1,
                Protocol = SendingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.Password,
            }
        );
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());

        db.ChangeTracker.Clear();
        db.ReceivingAccounts.Add(
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("invalid-receiving@example.com"),
                Protocol = ReceivingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.Password,
            }
        );
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ImapPasswordCredential_RequiresPasswordAndRejectsOAuthCombination()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        var service = new AccountCredentialService(db, new FakeCredentialProtector());
        var receiving = new ReceivingAccount
        {
            AuthenticationMethod = AuthenticationMethod.Password,
        };

        await Assert.ThrowsExactlyAsync<KnownException>(
            () =>
                service.SetImapCredentialAsync(
                    receiving,
                    new ProtocolCredentialInput(
                        "imap.example.com",
                        993,
                        ConnectionSecurity.SSL,
                        "user@example.com",
                        null
                    )
                )
        );

        receiving.AuthenticationMethod = AuthenticationMethod.OAuth2;
        await Assert.ThrowsExactlyAsync<KnownException>(
            () =>
                service.SetImapCredentialAsync(
                    receiving,
                    new ProtocolCredentialInput(
                        "imap.example.com",
                        993,
                        ConnectionSecurity.SSL,
                        "user@example.com",
                        "secret"
                    )
                )
        );
    }

    [TestMethod]
    public void OrdinaryResponseDtos_DoNotExposeSecretsOrTokens()
    {
        string[] forbiddenFragments = ["Password", "Secret", "AccessToken", "RefreshToken"];
        foreach (var dtoType in new[] { typeof(SenderAccountDto), typeof(ReceivingAccountDto) })
        {
            var propertyNames = dtoType.GetProperties().Select(x => x.Name).ToList();
            Assert.IsFalse(
                propertyNames.Any(name =>
                    forbiddenFragments.Any(fragment =>
                        name.Contains(fragment, StringComparison.OrdinalIgnoreCase)
                    )
                ),
                $"{dtoType.Name} 暴露了敏感字段"
            );
        }
    }

    private static async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        await connection.OpenAsync();
        return connection;
    }

    private static SqlContext CreateContext(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<SqlContext>().UseSqlite(connection).Options);

    private static EmailAccount CreateEmailAccount(string email) =>
        new()
        {
            UserId = 10,
            OrganizationId = 20,
            Email = email,
        };

    private static SenderAccount CreateSenderAccount(string email) =>
        new()
        {
            EmailAccount = CreateEmailAccount(email),
            EmailGroupId = 1,
            Protocol = SendingProtocol.Smtp,
            AuthenticationMethod = AuthenticationMethod.Password,
        };

    private static void AssertUniqueIndex<TEntity>(SqlContext db, params string[] propertyNames)
    {
        var hasIndex = db
            .Model.FindEntityType(typeof(TEntity))!
            .GetIndexes()
            .Any(index =>
                index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(propertyNames)
            );
        Assert.IsTrue(hasIndex, $"{typeof(TEntity).Name} 缺少唯一索引");
    }

    private sealed class FakeCredentialProtector : ICredentialProtector
    {
        public string ActiveKeyVersion => "test";

        public ProtectedCredential Protect(string plaintext) => new(plaintext, ActiveKeyVersion);

        public string Unprotect(string ciphertext, string keyVersion) => ciphertext;
    }
}
