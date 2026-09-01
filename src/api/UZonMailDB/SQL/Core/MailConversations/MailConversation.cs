using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 一个自有邮箱账号与一个联系人或参与人组之间的长期沟通时间线。
/// </summary>
public sealed class MailConversation : UserAndOrgId, IEntityTypeConfiguration<MailConversation>
{
    public long EmailAccountId { get; set; }
    public EmailAccount EmailAccount { get; set; } = null!;
    public MailConversationType ConversationType { get; set; }
    public string ParticipantSetKey { get; set; } = string.Empty;
    public string? DisplayTitle { get; set; }
    public DateTime LastMessageAtUtc { get; set; }
    public string? LastMessagePreview { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastReadAtUtc { get; set; }
    public List<MailConversationParticipant> Participants { get; set; } = [];
    public List<MailConversationMessage> Messages { get; set; } = [];

    public void Configure(EntityTypeBuilder<MailConversation> builder)
    {
        builder.ToTable("MailConversations");
        builder.Property(x => x.ParticipantSetKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayTitle).HasMaxLength(500);
        builder.Property(x => x.LastMessagePreview).HasMaxLength(500);
        builder
            .HasIndex(x => new
            {
                x.UserId,
                x.EmailAccountId,
                x.ParticipantSetKey
            })
            .IsUnique();
        builder.HasIndex(x => new
        {
            x.UserId,
            x.LastMessageAtUtc,
            x.Id
        });
        builder
            .HasOne(x => x.EmailAccount)
            .WithMany()
            .HasForeignKey(x => x.EmailAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
