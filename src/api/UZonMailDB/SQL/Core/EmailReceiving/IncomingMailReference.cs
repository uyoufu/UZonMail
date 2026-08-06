using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件线程头中引用的互联网消息标识。
/// </summary>
public class IncomingMailReference : SqlId, IEntityTypeConfiguration<IncomingMailReference>
{
    /// <summary>
    /// 所属入站邮件的标识。
    /// </summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>
    /// 所属入站邮件。
    /// </summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>
    /// 引用来自 In-Reply-To 还是 References 头。
    /// </summary>
    public IncomingMailReferenceType ReferenceType { get; set; }

    /// <summary>
    /// 被引用的 RFC Message-ID 原始值。
    /// </summary>
    public string InternetMessageId { get; set; } = string.Empty;

    /// <summary>
    /// 该消息标识在对应引用头中的出现顺序，从零开始。
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 配置线程引用的顺序唯一性和按消息标识匹配的索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailReference> builder)
    {
        builder.ToTable("IncomingMailReferences");
        builder.Property(x => x.InternetMessageId).HasMaxLength(1000).IsRequired();
        builder
            .HasIndex(x => new
            {
                x.IncomingMailMessageId,
                x.ReferenceType,
                x.Position
            })
            .IsUnique();
        builder.HasIndex(x => x.InternetMessageId);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.References)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
