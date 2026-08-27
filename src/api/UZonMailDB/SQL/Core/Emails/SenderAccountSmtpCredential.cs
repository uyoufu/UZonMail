using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Emails;

public class SenderAccountSmtpCredential
    : SqlId,
        IEntityTypeConfiguration<SenderAccountSmtpCredential>
{
    public long SenderAccountId { get; set; }
    public SenderAccount SenderAccount { get; set; } = null!;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public ConnectionSecurity ConnectionSecurity { get; set; }
    public string LoginName { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public string EncryptionKeyVersion { get; set; } = string.Empty;
    public DateTime CredentialUpdatedAtUtc { get; set; }

    public void Configure(EntityTypeBuilder<SenderAccountSmtpCredential> builder)
    {
        builder.ToTable("SenderAccountSmtpCredentials");
        builder.Property(x => x.Host).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LoginName).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EncryptionKeyVersion).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.SenderAccountId).IsUnique();
        builder
            .HasOne(x => x.SenderAccount)
            .WithOne(x => x.SmtpCredential)
            .HasForeignKey<SenderAccountSmtpCredential>(x => x.SenderAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
