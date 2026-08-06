using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 一次邮件分析产生的单项分类结论。
/// 通过独立记录保留多分类和历史重跑结果。
/// </summary>
public class IncomingMailAnalysisClassification
    : SqlId,
        IEntityTypeConfiguration<IncomingMailAnalysisClassification>
{
    /// <summary>所属分析运行。</summary>
    public long IncomingMailAnalysisId { get; set; }

    /// <summary>所属分析运行导航。</summary>
    public IncomingMailAnalysis IncomingMailAnalysis { get; set; } = null!;

    /// <summary>分类结论。</summary>
    public IncomingMailClassification Classification { get; set; }

    /// <summary>结论置信度，范围为 0 到 1。</summary>
    public decimal Confidence { get; set; }

    /// <summary>配置单次分析中分类结论的唯一性。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailAnalysisClassification> builder)
    {
        builder.ToTable("IncomingMailAnalysisClassifications");
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.HasIndex(x => new { x.IncomingMailAnalysisId, x.Classification }).IsUnique();
        builder
            .HasOne(x => x.IncomingMailAnalysis)
            .WithMany(x => x.Classifications)
            .HasForeignKey(x => x.IncomingMailAnalysisId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
