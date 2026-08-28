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
    private const string RemovedSenderWeightPropertyName = "Weight";

    [TestMethod]
    public async Task Model_RegistersUnifiedIdentityAndSeparatedCapabilities()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        Assert.IsNotNull(db.Model.FindEntityType(typeof(EmailAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(SenderAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ReceivingAccount)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(SenderAccountSmtpCredential)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(ReceivingAccountImapCredential)));
        Assert.IsNotNull(db.Model.FindEntityType(typeof(EmailAccountOAuthCredential)));
        Assert.IsNull(
            db.Model.FindEntityType(typeof(SenderAccount))!
                .FindProperty(RemovedSenderWeightPropertyName)
        );
        Assert.IsFalse(
            db.Model.GetEntityTypes()
                .Any(x =>
                    x.ClrType?.Name
                        is "ReceivingAccountSenderLink"
                            or "ReceivingAccountPrimarySender"
                )
        );

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
        Assert.IsTrue(
            db.Model.FindEntityType(typeof(EmailAccount))!
                .GetForeignKeys()
                .Any(foreignKey =>
                    foreignKey.Properties.Single().Name == nameof(EmailAccount.EmailGroupId)
                    && foreignKey.PrincipalEntityType.ClrType == typeof(EmailGroup)
                )
        );
    }

    [TestMethod]
    public async Task EmailAccount_EnforcesNormalizedAddressUniquenessPerUser()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var group = CreateEmailAccountGroup();
        db.EmailGroups.Add(group);
        db.EmailAccounts.Add(CreateEmailAccount(" Sender@Example.com ", group.Id));
        await db.SaveChangesAsync();
        db.EmailAccounts.Add(CreateEmailAccount("sender@example.com", group.Id));

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ProtocolAuthenticationConstraints_AcceptSupportedAndRejectInvalidCombinations()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var group = CreateEmailAccountGroup();
        db.AddRange(
            group,
            new SenderAccount
            {
                EmailAccount = CreateEmailAccount("smtp@example.com", group.Id),
                Protocol = SendingProtocol.Smtp,
                AuthenticationMethod = AuthenticationMethod.Password,
            },
            new SenderAccount
            {
                EmailAccount = CreateEmailAccount("graph-sender@example.com", group.Id),
                Protocol = SendingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.OAuth2,
            },
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("imap@example.com", group.Id),
                Protocol = ReceivingProtocol.Imap,
                AuthenticationMethod = AuthenticationMethod.Password,
            },
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("graph-receiving@example.com", group.Id),
                Protocol = ReceivingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.OAuth2,
            }
        );
        await db.SaveChangesAsync();

        db.SenderAccounts.Add(
            new SenderAccount
            {
                EmailAccount = CreateEmailAccount("invalid-sender@example.com", group.Id),
                Protocol = SendingProtocol.MicrosoftGraph,
                AuthenticationMethod = AuthenticationMethod.Password,
            }
        );
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());

        db.ChangeTracker.Clear();
        db.ReceivingAccounts.Add(
            new ReceivingAccount
            {
                EmailAccount = CreateEmailAccount("invalid-receiving@example.com", group.Id),
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
                    ),
                    "user@example.com"
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
                    ),
                    "user@example.com"
                )
        );
    }

    [TestMethod]
    public async Task PasswordCredentials_UseAccountEmailWhenLoginNameIsBlank()
    {
        await using var connection = await OpenConnectionAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        var service = new AccountCredentialService(db, new FakeCredentialProtector());

        await service.SetSmtpCredentialAsync(
            new SenderAccount(),
            new ProtocolCredentialInput(
                "smtp.example.com",
                465,
                ConnectionSecurity.SSL,
                " ",
                "smtp-secret"
            ),
            "sender@example.com"
        );
        await service.SetImapCredentialAsync(
            new ReceivingAccount { AuthenticationMethod = AuthenticationMethod.Password },
            new ProtocolCredentialInput(
                "imap.example.com",
                993,
                ConnectionSecurity.SSL,
                null,
                "imap-secret"
            ),
            "receiver@example.com"
        );

        Assert.AreEqual(
            "sender@example.com",
            db.SenderAccountSmtpCredentials.Local.Single().LoginName
        );
        Assert.AreEqual(
            "receiver@example.com",
            db.ReceivingAccountImapCredentials.Local.Single().LoginName
        );
    }

    [TestMethod]
    public void AccountResponseDtos_DoNotExposeSecretsOrTokens()
    {
        string[] forbiddenFragments = ["Password", "Secret", "AccessToken", "RefreshToken"];
        foreach (
            var dtoType in new[]
            {
                typeof(EmailAccountDto),
                typeof(EmailAccountSenderCapabilityDto),
                typeof(EmailAccountReceivingCapabilityDto),
                typeof(EmailAccountProtocolCredentialDto),
            }
        )
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

    private static EmailGroup CreateEmailAccountGroup() =>
        new()
        {
            Id = 1,
            UserId = 10,
            Category = EmailGroupCategory.EmailAccount,
            Name = "Accounts",
        };

    private static EmailAccount CreateEmailAccount(string email, long emailGroupId) =>
        new()
        {
            UserId = 10,
            OrganizationId = 20,
            EmailGroupId = emailGroupId,
            Email = email,
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
