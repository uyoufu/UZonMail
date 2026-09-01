using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 当前用户在邮件会话中识别的外部联系人。
/// </summary>
public sealed class MailContact : UserAndOrgId, IEntityTypeConfiguration<MailContact>
{
    private string _email = string.Empty;

    public string Email
    {
        get => _email;
        set
        {
            _email = value.Trim();
            NormalizedEmail = _email.ToLowerInvariant();
        }
    }

    public string NormalizedEmail { get; private set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime LastInteractionAtUtc { get; set; }
    public List<MailContactTag> Tags { get; set; } = [];
    public List<MailConversationParticipant> ConversationParticipants { get; set; } = [];

    public void Configure(EntityTypeBuilder<MailContact> builder)
    {
        builder.ToTable("MailContacts");
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(500);
        builder.HasIndex(x => new { x.UserId, x.NormalizedEmail }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.LastInteractionAtUtc });
    }
}
