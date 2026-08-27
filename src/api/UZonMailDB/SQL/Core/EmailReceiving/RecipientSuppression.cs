using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

public enum RecipientSuppressionReason
{
    Unsubscribe = 0,
    HardBounce = 1,
    Complaint = 2,
    Manual = 3,
}

/// <summary>
/// 组织级统一停发记录。
/// </summary>
public sealed class RecipientSuppression : OrgId, IEntityTypeConfiguration<RecipientSuppression>
{
    private string _email = string.Empty;

    public string Email
    {
        get => _email;
        set => _email = value.Trim().ToLowerInvariant();
    }

    public RecipientSuppressionReason Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedByUserId { get; set; }
    public DateTime SuppressedAtUtc { get; set; }
    public long? ReleasedByUserId { get; set; }
    public DateTime? ReleasedAtUtc { get; set; }
    public string? ReleaseReason { get; set; }

    public void Configure(EntityTypeBuilder<RecipientSuppression> builder)
    {
        builder.ToTable("RecipientSuppressions");
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.ReasonDetail).HasMaxLength(1000);
        builder.Property(x => x.ReleaseReason).HasMaxLength(1000);
        builder.HasIndex(x => new { x.OrganizationId, x.Email }).IsUnique();
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.IsActive,
            x.Email,
        });
    }
}
