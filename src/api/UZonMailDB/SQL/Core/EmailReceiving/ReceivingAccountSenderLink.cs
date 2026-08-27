using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

public class ReceivingAccountSenderLink
    : SqlId,
        IEntityTypeConfiguration<ReceivingAccountSenderLink>
{
    public long ReceivingAccountId { get; set; }
    public ReceivingAccount ReceivingAccount { get; set; } = null!;
    public long SenderAccountId { get; set; }
    public SenderAccount SenderAccount { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ReceivingAccountSenderLink> builder)
    {
        builder.ToTable("ReceivingAccountSenderLinks");
        builder.HasAlternateKey(x => new { x.Id, x.ReceivingAccountId });
        builder.HasIndex(x => new { x.ReceivingAccountId, x.SenderAccountId }).IsUnique();
        builder.HasIndex(x => x.SenderAccountId);
        builder
            .HasOne(x => x.ReceivingAccount)
            .WithMany(x => x.SenderLinks)
            .HasForeignKey(x => x.ReceivingAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.SenderAccount)
            .WithMany()
            .HasForeignKey(x => x.SenderAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
