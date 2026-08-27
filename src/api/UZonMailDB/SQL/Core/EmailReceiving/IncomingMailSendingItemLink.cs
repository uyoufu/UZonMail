using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件与已发送邮件之间的可审核归因关联。
/// </summary>
public class IncomingMailSendingItemLink
    : SqlId,
        IEntityTypeConfiguration<IncomingMailSendingItemLink>
{
    /// <summary>
    /// 入站邮件的标识。
    /// </summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>
    /// 入站邮件。
    /// </summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>
    /// 被归因的发件项标识。
    /// </summary>
    public long SendingItemId { get; set; }

    /// <summary>
    /// 被归因的发件项。
    /// </summary>
    public SendingItem SendingItem { get; set; } = null!;

    /// <summary>
    /// 被归因的具体发送收件人；批量发送或 DSN 含多个收件人时使用。
    /// 无法精确定位收件人时为空。
    /// </summary>
    public long? SendingItemRecipientId { get; set; }

    /// <summary>
    /// 被归因的具体发送收件人导航。
    /// </summary>
    public SendingItemRecipient? SendingItemRecipient { get; set; }

    /// <summary>
    /// 该关联代表回复、投递状态通知还是垃圾邮件投诉。
    /// </summary>
    public IncomingMailLinkType LinkType { get; set; }

    /// <summary>
    /// 产生关联的匹配依据。
    /// </summary>
    public IncomingMailLinkMatchMethod MatchMethod { get; set; }

    /// <summary>
    /// 自动匹配的置信度，范围为 0 到 1；人工关联固定为 1。
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// 关联当前的审核状态。
    /// </summary>
    public IncomingMailLinkStatus Status { get; set; } = IncomingMailLinkStatus.Proposed;

    /// <summary>
    /// 审核关联的用户标识；自动关联未审核时为空。
    /// </summary>
    public long? ReviewedByUserId { get; set; }

    /// <summary>
    /// 审核关联的 UTC 时间；未审核时为空。
    /// </summary>
    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>
    /// 匹配依据的简短说明，例如命中的 Message-ID 或规则名称。
    /// </summary>
    public string? MatchReason { get; set; }

    /// <summary>
    /// 配置归因关联的唯一性及统计查询索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailSendingItemLink> builder)
    {
        builder.ToTable("IncomingMailSendingItemLinks");
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.MatchReason).HasMaxLength(1000);
        builder
            .HasIndex(x => new
            {
                x.IncomingMailMessageId,
                x.SendingItemId,
                x.SendingItemRecipientId,
                x.LinkType,
            })
            .IsUnique();
        builder.HasIndex(x => new
        {
            x.SendingItemId,
            x.LinkType,
            x.Status
        });
        builder.HasIndex(x => x.SendingItemRecipientId);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.SendingItemLinks)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.SendingItem)
            .WithMany()
            .HasForeignKey(x => x.SendingItemId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.SendingItemRecipient)
            .WithMany()
            .HasForeignKey(x => x.SendingItemRecipientId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
