using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件当前生效的单项分类，供业务列表和统计直接查询。
/// </summary>
public class IncomingMailCurrentClassification
    : SqlId,
        IEntityTypeConfiguration<IncomingMailCurrentClassification>
{
    /// <summary>所属入站邮件。</summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>所属入站邮件导航。</summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>产生当前结论的分析运行。</summary>
    public long IncomingMailAnalysisId { get; set; }

    /// <summary>产生当前结论的分析运行导航。</summary>
    public IncomingMailAnalysis IncomingMailAnalysis { get; set; } = null!;

    /// <summary>当前生效的分类。</summary>
    public IncomingMailClassification Classification { get; set; }

    /// <summary>当前结论置信度，范围为 0 到 1。</summary>
    public decimal Confidence { get; set; }

    /// <summary>结论最后生效的 UTC 时间。</summary>
    public DateTime AppliedAtUtc { get; set; }

    /// <summary>配置一封邮件每种当前分类仅一条记录。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailCurrentClassification> builder)
    {
        builder.ToTable("IncomingMailCurrentClassifications");
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.HasIndex(x => new { x.IncomingMailMessageId, x.Classification }).IsUnique();
        builder.HasIndex(x => new { x.Classification, x.AppliedAtUtc });
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.CurrentClassifications)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.IncomingMailAnalysis)
            .WithMany()
            .HasForeignKey(x => x.IncomingMailAnalysisId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
