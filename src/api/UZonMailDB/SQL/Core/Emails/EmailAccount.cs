using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.EmailReceiving;

namespace UzonMail.DB.SQL.Core.Emails;

/// <summary>
/// 用户拥有的邮箱身份。发送和接收能力通过组合关系独立维护。
/// </summary>
public class EmailAccount : UserAndOrgId, IEntityTypeConfiguration<EmailAccount>
{
    private string _email = string.Empty;

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
    public SenderAccount? SenderAccount { get; set; }
    public ReceivingAccount? ReceivingAccount { get; set; }
    public EmailAccountOAuthCredential? OAuthCredential { get; set; }

    public void Configure(EntityTypeBuilder<EmailAccount> builder)
    {
        builder.ToTable("EmailAccounts");
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Domain).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Remark).HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.NormalizedEmail }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.Domain });
    }
}
