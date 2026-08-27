using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 邮箱身份的收信能力与同步策略。
/// </summary>
public class ReceivingAccount : SqlId, IEntityTypeConfiguration<ReceivingAccount>
{
    public long EmailAccountId { get; set; }
    public EmailAccount EmailAccount { get; set; } = null!;
    public ReceivingProtocol Protocol { get; set; }
    public AuthenticationMethod AuthenticationMethod { get; set; }
    public ReceivingAccountStatus Status { get; set; }
    public int ContentRetentionDays { get; set; } = 30;
    public DateTime? LastSuccessfulSyncAtUtc { get; set; }
    public DateTime? LastSyncAttemptAtUtc { get; set; }
    public DateTime? NextSyncAtUtc { get; set; }
    public DateTime? LastConnectedAtUtc { get; set; }
    public string? LastError { get; set; }
    public List<ReceivingAccountSenderLink> SenderLinks { get; set; } = [];
    public ReceivingAccountPrimarySender? PrimarySender { get; set; }
    public List<ImapMailbox> Mailboxes { get; set; } = [];

    public void Configure(EntityTypeBuilder<ReceivingAccount> builder)
    {
        builder.ToTable(
            "ReceivingAccounts",
            table =>
                table.HasCheckConstraint(
                    "CK_ReceivingAccounts_ProtocolAuthentication",
                    "(\"Protocol\" = 0 AND \"AuthenticationMethod\" IN (0, 1)) OR (\"Protocol\" = 1 AND \"AuthenticationMethod\" = 1)"
                )
        );
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasIndex(x => x.EmailAccountId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextSyncAtUtc });
        builder
            .HasOne(x => x.EmailAccount)
            .WithOne(x => x.ReceivingAccount)
            .HasForeignKey<ReceivingAccount>(x => x.EmailAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
