using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.Emails;

/// <summary>
/// 用于按邮箱域名推断 IMAP 连接参数的系统元数据。
/// </summary>
public sealed class ImapInfo : SqlId, IMailServerInfo, IEntityTypeConfiguration<ImapInfo>
{
    public string Domain { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;

    public void Configure(EntityTypeBuilder<ImapInfo> builder)
    {
        builder.ToTable("ImapInfos");
        builder.Property(x => x.Domain).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Host).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Domain).IsUnique();
    }
}
