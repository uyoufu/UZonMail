using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 会话时间线中的一封邮件，正文仍由同步消息或发件项持有。
/// </summary>
public sealed class MailConversationMessage
    : SqlId,
        IEntityTypeConfiguration<MailConversationMessage>
{
    public long MailConversationId { get; set; }
    public MailConversation MailConversation { get; set; } = null!;
    public long? IncomingMailMessageId { get; set; }
    public IncomingMailMessage? IncomingMailMessage { get; set; }
    public long? SendingItemId { get; set; }
    public SendingItem? SendingItem { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public MailMessageDirection Direction { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public bool IsRead { get; set; }
    public long? ReplyToConversationMessageId { get; set; }
    public MailConversationMessage? ReplyToConversationMessage { get; set; }

    public void Configure(EntityTypeBuilder<MailConversationMessage> builder)
    {
        builder.ToTable("MailConversationMessages");
        builder.Property(x => x.SourceKey).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.MailConversationId, x.SourceKey }).IsUnique();
        builder.HasIndex(x => new
        {
            x.MailConversationId,
            x.OccurredAtUtc,
            x.Id
        });
        builder.HasIndex(x => x.IncomingMailMessageId);
        builder.HasIndex(x => x.SendingItemId);
        builder
            .HasOne(x => x.MailConversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.MailConversationId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany()
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.SendingItem)
            .WithMany()
            .HasForeignKey(x => x.SendingItemId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ReplyToConversationMessage)
            .WithMany()
            .HasForeignKey(x => x.ReplyToConversationMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
