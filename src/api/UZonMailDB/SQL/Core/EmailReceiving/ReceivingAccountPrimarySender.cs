using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

public class ReceivingAccountPrimarySender
    : SqlId,
        IEntityTypeConfiguration<ReceivingAccountPrimarySender>
{
    public long ReceivingAccountId { get; set; }
    public ReceivingAccount ReceivingAccount { get; set; } = null!;
    public long ReceivingAccountSenderLinkId { get; set; }
    public ReceivingAccountSenderLink ReceivingAccountSenderLink { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ReceivingAccountPrimarySender> builder)
    {
        builder.ToTable("ReceivingAccountPrimarySenders");
        builder.HasIndex(x => x.ReceivingAccountId).IsUnique();
        builder
            .HasOne(x => x.ReceivingAccount)
            .WithOne(x => x.PrimarySender)
            .HasForeignKey<ReceivingAccountPrimarySender>(x => x.ReceivingAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ReceivingAccountSenderLink)
            .WithMany()
            .HasForeignKey(x => new { x.ReceivingAccountSenderLinkId, x.ReceivingAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ReceivingAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
