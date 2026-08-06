using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 支撑邮件分类结论的可审计证据定位信息。
/// 仅保存位置、摘要和哈希，避免在审计表中复制邮件正文。
/// </summary>
public class IncomingMailClassificationEvidence
    : SqlId,
        IEntityTypeConfiguration<IncomingMailClassificationEvidence>
{
    /// <summary>所属分析运行。</summary>
    public long IncomingMailAnalysisId { get; set; }

    /// <summary>所属分析运行导航。</summary>
    public IncomingMailAnalysis IncomingMailAnalysis { get; set; } = null!;

    /// <summary>证据的来源类型。</summary>
    public IncomingMailEvidenceType EvidenceType { get; set; }

    /// <summary>邮件头名称、MIME 路径或报告字段名称。</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>不含邮件正文的证据摘要。</summary>
    public string? Summary { get; set; }

    /// <summary>证据原值的 SHA-256；无须保存原值时为空。</summary>
    public string? ValueSha256 { get; set; }

    /// <summary>配置分析证据的审计查询索引。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailClassificationEvidence> builder)
    {
        builder.ToTable("IncomingMailClassificationEvidences");
        builder.Property(x => x.Location).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.ValueSha256).HasMaxLength(64);
        builder.HasIndex(x => new { x.IncomingMailAnalysisId, x.EvidenceType });
        builder
            .HasOne(x => x.IncomingMailAnalysis)
            .WithMany(x => x.Evidences)
            .HasForeignKey(x => x.IncomingMailAnalysisId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
