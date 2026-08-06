using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 一个 IMAP 账户选择的主要发件箱关联。
/// 独立实体保证每个账户最多选择一个，且所选关联属于该账户。
/// </summary>
public class ImapAccountPrimaryOutbox : SqlId, IEntityTypeConfiguration<ImapAccountPrimaryOutbox>
{
    /// <summary>所属 IMAP 账户。</summary>
    public long ImapAccountId { get; set; }

    /// <summary>所属 IMAP 账户导航。</summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>被选为主发件箱的账户关联记录。</summary>
    public long ImapAccountOutboxLinkId { get; set; }

    /// <summary>被选为主发件箱的账户关联记录导航。</summary>
    public ImapAccountOutboxLink ImapAccountOutboxLink { get; set; } = null!;

    /// <summary>配置每个账户至多一个主发件箱，并约束关联的账户作用域。</summary>
    public void Configure(EntityTypeBuilder<ImapAccountPrimaryOutbox> builder)
    {
        builder.ToTable("ImapAccountPrimaryOutboxes");
        builder.HasIndex(x => x.ImapAccountId).IsUnique();
        builder.HasIndex(x => x.ImapAccountOutboxLinkId).IsUnique();
        builder
            .HasOne(x => x.ImapAccount)
            .WithOne(x => x.PrimaryOutbox)
            .HasForeignKey<ImapAccountPrimaryOutbox>(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapAccountOutboxLink)
            .WithMany()
            .HasForeignKey(x => new { x.ImapAccountOutboxLinkId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
