using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 一次可追溯、可重新运行的入站邮件分析结果。
/// </summary>
public class IncomingMailAnalysis : SqlId, IEntityTypeConfiguration<IncomingMailAnalysis>
{
    /// <summary>
    /// 被分析入站邮件的标识。
    /// </summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>
    /// 被分析入站邮件。
    /// </summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>
    /// 分析结果的生成来源。
    /// </summary>
    public IncomingMailAnalysisSource Source { get; set; }

    /// <summary>
    /// 规则集、模型或人工流程的版本标识。
    /// </summary>
    public string AnalyzerVersion { get; set; } = string.Empty;

    /// <summary>
    /// 本次分析的执行状态。
    /// </summary>
    public IncomingMailAnalysisStatus Status { get; set; }

    /// <summary>
    /// 本次分析得出的退信类型。
    /// </summary>
    public IncomingMailBounceType BounceType { get; set; }

    /// <summary>
    /// 本次分析得出的垃圾邮件评分；未产生评分时为空。
    /// </summary>
    public decimal? SpamScore { get; set; }

    /// <summary>
    /// 适合审计查看的结论摘要，不保存邮件正文。
    /// </summary>
    public string? ResultSummary { get; set; }

    /// <summary>
    /// 分析失败的无敏感信息原因；成功时为空。
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 开始分析的 UTC 时间。
    /// </summary>
    public DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// 完成分析的 UTC 时间；未结束时为空。
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 本次分析输入内容的 SHA-256，用于判断重跑是否使用同一邮件版本。
    /// </summary>
    public string? InputContentSha256 { get; set; }

    /// <summary>
    /// 本次分析产生的多项分类结论。
    /// </summary>
    public List<IncomingMailAnalysisClassification> Classifications { get; set; } = [];

    /// <summary>
    /// 本次分析解析出的 DSN 收件人状态。
    /// </summary>
    public List<IncomingMailDeliveryStatus> DeliveryStatuses { get; set; } = [];

    /// <summary>
    /// 本次分析解析出的 ARF 投诉报告。
    /// </summary>
    public List<IncomingMailFeedbackReport> FeedbackReports { get; set; } = [];

    /// <summary>
    /// 本次分析使用的可审计证据集合。
    /// </summary>
    public List<IncomingMailClassificationEvidence> Evidences { get; set; } = [];

    /// <summary>
    /// 配置分析时间线及按当前分类回溯的索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailAnalysis> builder)
    {
        builder.ToTable("IncomingMailAnalyses");
        builder.Property(x => x.AnalyzerVersion).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SpamScore).HasPrecision(5, 2);
        builder.Property(x => x.ResultSummary).HasMaxLength(2000);
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.InputContentSha256).HasMaxLength(64);
        builder.HasIndex(x => new { x.IncomingMailMessageId, x.StartedAtUtc });
        builder.HasIndex(x => new { x.Status, x.StartedAtUtc });
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.Analyses)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
