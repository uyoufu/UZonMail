using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

public class ReceivingAccountImapCredential
    : SqlId,
        IEntityTypeConfiguration<ReceivingAccountImapCredential>
{
    public long ReceivingAccountId { get; set; }
    public ReceivingAccount ReceivingAccount { get; set; } = null!;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;
    public string LoginName { get; set; } = string.Empty;
    public string? EncryptedPassword { get; set; }
    public string? EncryptionKeyVersion { get; set; }
    public DateTime CredentialUpdatedAtUtc { get; set; }

    public void Configure(EntityTypeBuilder<ReceivingAccountImapCredential> builder)
    {
        builder.ToTable("ReceivingAccountImapCredentials");
        builder.Property(x => x.Host).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LoginName).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EncryptionKeyVersion).HasMaxLength(100);
        builder.HasIndex(x => x.ReceivingAccountId).IsUnique();
        builder
            .HasOne(x => x.ReceivingAccount)
            .WithOne()
            .HasForeignKey<ReceivingAccountImapCredential>(x => x.ReceivingAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
