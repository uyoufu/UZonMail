using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 会话中的外部联系人及其当前成员状态。
/// </summary>
public sealed class MailConversationParticipant
    : SqlId,
        IEntityTypeConfiguration<MailConversationParticipant>
{
    public long MailConversationId { get; set; }
    public MailConversation MailConversation { get; set; } = null!;
    public long MailContactId { get; set; }
    public MailContact MailContact { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAtUtc { get; set; }
    public DateTime? LeftAtUtc { get; set; }

    public void Configure(EntityTypeBuilder<MailConversationParticipant> builder)
    {
        builder.ToTable("MailConversationParticipants");
        builder.HasIndex(x => new { x.MailConversationId, x.MailContactId }).IsUnique();
        builder.HasIndex(x => new { x.MailContactId, x.IsActive });
        builder
            .HasOne(x => x.MailConversation)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.MailConversationId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.MailContact)
            .WithMany(x => x.ConversationParticipants)
            .HasForeignKey(x => x.MailContactId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
