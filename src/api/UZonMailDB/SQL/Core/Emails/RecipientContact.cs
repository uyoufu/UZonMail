using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Emails;

/// <summary>
/// 用户维护的邮件投递目标，不承载任何登录或认证语义。
/// </summary>
public class RecipientContact : UserAndOrgId, IEntityTypeConfiguration<RecipientContact>
{
    private string _email = string.Empty;

    public long EmailGroupId { get; set; }
    public EmailGroup EmailGroup { get; set; } = null!;
    public string Email
    {
        get => _email;
        set
        {
            _email = value.Trim();
            NormalizedEmail = _email.ToLowerInvariant();
            Domain = NormalizedEmail.Split('@').LastOrDefault();
        }
    }

    public string NormalizedEmail { get; private set; } = string.Empty;
    public string? Domain { get; private set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    public DateTime LastSuccessDeliveryDate { get; set; }
    public DateTime LastDeliveredAtUtc { get; set; }
    public long MinimumCooldownHours { get; set; } = -1;
    public RecipientValidationStatus ValidationStatus { get; set; }
    public string? ValidationFailureReason { get; set; }

    public void Configure(EntityTypeBuilder<RecipientContact> builder)
    {
        builder.ToTable("RecipientContacts");
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Domain).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Remark).HasMaxLength(2000);
        builder.Property(x => x.ValidationFailureReason).HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.NormalizedEmail }).IsUnique();
        builder.HasIndex(x => new
        {
            x.EmailGroupId,
            x.ValidationStatus,
            x.Id
        });
        builder
            .HasOne(x => x.EmailGroup)
            .WithMany(x => x.RecipientContacts)
            .HasForeignKey(x => x.EmailGroupId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
