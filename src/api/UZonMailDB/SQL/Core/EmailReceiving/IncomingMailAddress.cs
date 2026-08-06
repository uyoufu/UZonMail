using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件头中一个已规范化的邮箱地址。
/// </summary>
public class IncomingMailAddress : SqlId, IEntityTypeConfiguration<IncomingMailAddress>
{
    private string _email = string.Empty;

    /// <summary>
    /// 所属入站邮件的标识。
    /// </summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>
    /// 所属入站邮件。
    /// </summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>
    /// 地址在邮件头中的业务角色。
    /// </summary>
    public IncomingMailAddressType AddressType { get; set; }

    /// <summary>
    /// 规范化后的邮箱地址；写入时统一去除空格并转为小写。
    /// </summary>
    public string Email
    {
        get => _email;
        set => _email = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 邮件头中提供的显示名称。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 同一地址角色下的原始出现顺序，从零开始。
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 配置地址的顺序唯一性和按邮箱检索的索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailAddress> builder)
    {
        builder.ToTable("IncomingMailAddresses");
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(500);
        builder
            .HasIndex(x => new
            {
                x.IncomingMailMessageId,
                x.AddressType,
                x.Position
            })
            .IsUnique();
        builder.HasIndex(x => x.Email);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
