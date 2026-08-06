using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 服务器为邮件位置附加的自定义关键字。
/// </summary>
public class IncomingMailKeyword : SqlId, IEntityTypeConfiguration<IncomingMailKeyword>
{
    /// <summary>
    /// 所属邮件位置的标识。
    /// </summary>
    public long IncomingMailLocationId { get; set; }

    /// <summary>
    /// 所属邮件位置。
    /// </summary>
    public IncomingMailLocation IncomingMailLocation { get; set; } = null!;

    /// <summary>
    /// 服务器提供的关键字文本。
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// 配置单个邮件位置上的关键字唯一性。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailKeyword> builder)
    {
        builder.ToTable("IncomingMailKeywords");
        builder.Property(x => x.Keyword).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => new { x.IncomingMailLocationId, x.Keyword }).IsUnique();
        builder
            .HasOne(x => x.IncomingMailLocation)
            .WithMany(x => x.Keywords)
            .HasForeignKey(x => x.IncomingMailLocationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
