using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 联系人与用户标签的显式关联。
/// </summary>
public sealed class MailContactTag : SqlId, IEntityTypeConfiguration<MailContactTag>
{
    public long MailContactId { get; set; }
    public MailContact MailContact { get; set; } = null!;
    public long MailTagId { get; set; }
    public MailTag MailTag { get; set; } = null!;

    public void Configure(EntityTypeBuilder<MailContactTag> builder)
    {
        builder.ToTable("MailContactTags");
        builder.HasIndex(x => new { x.MailContactId, x.MailTagId }).IsUnique();
        builder
            .HasOne(x => x.MailContact)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.MailContactId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.MailTag)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.MailTagId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
