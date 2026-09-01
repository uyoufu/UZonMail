using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 当前用户用于标记邮件联系人的标签。
/// </summary>
public sealed class MailTag : UserAndOrgId, IEntityTypeConfiguration<MailTag>
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value.Trim();
            NormalizedName = _name.ToLowerInvariant();
        }
    }

    public string NormalizedName { get; private set; } = string.Empty;
    public string Color { get; set; } = "#1976d2";
    public List<MailContactTag> Contacts { get; set; } = [];

    public void Configure(EntityTypeBuilder<MailTag> builder)
    {
        builder.ToTable("MailTags");
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.NormalizedName }).IsUnique();
    }
}
