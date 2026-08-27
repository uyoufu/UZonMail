using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Emails;

/// <summary>
/// 邮箱身份的发送能力与调度策略。
/// </summary>
public class SenderAccount : SqlId, IEntityTypeConfiguration<SenderAccount>
{
    public long EmailAccountId { get; set; }
    public EmailAccount EmailAccount { get; set; } = null!;
    public long EmailGroupId { get; set; }
    public EmailGroup EmailGroup { get; set; } = null!;
    public SendingProtocol Protocol { get; set; }
    public AuthenticationMethod AuthenticationMethod { get; set; }
    public long? ProxyId { get; set; }
    public int MaxSendCountPerDay { get; set; }
    public int SentTotalToday { get; set; }
    public DateOnly? SentCountDateUtc { get; set; }
    public string? ReplyToEmails { get; set; }
    public int Weight { get; set; } = 1;
    public SenderAccountStatus Status { get; set; }
    public string? ValidationFailureReason { get; set; }
    public SenderAccountSmtpCredential? SmtpCredential { get; set; }

    [NotMapped]
    public long UserId => EmailAccount.UserId;

    [NotMapped]
    public long OrganizationId => EmailAccount.OrganizationId;

    [NotMapped]
    public string Email => EmailAccount.Email;

    [NotMapped]
    public string? Name => EmailAccount.Name;

    [NotMapped]
    public string? Description => EmailAccount.Description;

    public void Configure(EntityTypeBuilder<SenderAccount> builder)
    {
        builder.ToTable(
            "SenderAccounts",
            table =>
                table.HasCheckConstraint(
                    "CK_SenderAccounts_ProtocolAuthentication",
                    "(\"Protocol\" = 0 AND \"AuthenticationMethod\" = 0) OR (\"Protocol\" = 1 AND \"AuthenticationMethod\" = 1)"
                )
        );
        builder.Property(x => x.ReplyToEmails).HasMaxLength(2000);
        builder.Property(x => x.ValidationFailureReason).HasMaxLength(2000);
        builder.HasIndex(x => x.EmailAccountId).IsUnique();
        builder.HasIndex(x => new
        {
            x.EmailGroupId,
            x.Status,
            x.Id
        });
        builder
            .HasOne(x => x.EmailAccount)
            .WithOne(x => x.SenderAccount)
            .HasForeignKey<SenderAccount>(x => x.EmailAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.EmailGroup)
            .WithMany(x => x.SenderAccounts)
            .HasForeignKey(x => x.EmailGroupId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
