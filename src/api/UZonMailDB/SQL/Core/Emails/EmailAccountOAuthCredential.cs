using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Emails;

public class EmailAccountOAuthCredential
    : SqlId,
        IEntityTypeConfiguration<EmailAccountOAuthCredential>
{
    public long EmailAccountId { get; set; }
    public EmailAccount EmailAccount { get; set; } = null!;
    public OAuthProvider Provider { get; set; }
    public OAuthApplicationSource ApplicationSource { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? EncryptedClientSecret { get; set; }
    public string? EncryptedAccessToken { get; set; }
    public string? EncryptedRefreshToken { get; set; }
    public DateTime? AccessTokenExpiresAtUtc { get; set; }
    public string AuthorizedScopes { get; set; } = string.Empty;
    public string? TokenEndpoint { get; set; }
    public string EncryptionKeyVersion { get; set; } = string.Empty;
    public DateTime CredentialUpdatedAtUtc { get; set; }

    public void Configure(EntityTypeBuilder<EmailAccountOAuthCredential> builder)
    {
        builder.ToTable("EmailAccountOAuthCredentials");
        builder.Property(x => x.TenantId).HasMaxLength(500);
        builder.Property(x => x.ClientId).HasMaxLength(500);
        builder.Property(x => x.AuthorizedScopes).HasMaxLength(2000);
        builder.Property(x => x.TokenEndpoint).HasMaxLength(2000);
        builder.Property(x => x.EncryptionKeyVersion).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.EmailAccountId).IsUnique();
        builder
            .HasOne(x => x.EmailAccount)
            .WithOne(x => x.OAuthCredential)
            .HasForeignKey<EmailAccountOAuthCredential>(x => x.EmailAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
