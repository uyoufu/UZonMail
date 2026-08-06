using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 允许一个 IMAP 收件账户为多个 SMTP 发件箱收集回信的关联记录。
/// </summary>
public class ImapAccountOutboxLink : SqlId, IEntityTypeConfiguration<ImapAccountOutboxLink>
{
    /// <summary>
    /// IMAP 收件账户的数据库标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// IMAP 收件账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 可由此账户收取回信的 SMTP 发件箱标识。
    /// </summary>
    public long OutboxId { get; set; }

    /// <summary>
    /// 对应的 SMTP 发件箱。
    /// </summary>
    public Outbox Outbox { get; set; } = null!;

    /// <summary>
    /// 配置账户与发件箱的关联约束。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapAccountOutboxLink> builder)
    {
        builder.ToTable("ImapAccountOutboxLinks");
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder.HasIndex(x => new { x.ImapAccountId, x.OutboxId }).IsUnique();
        builder.HasIndex(x => x.OutboxId);
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany(x => x.OutboxLinks)
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.Outbox)
            .WithMany()
            .HasForeignKey(x => x.OutboxId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
