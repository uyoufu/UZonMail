using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 从 ARF feedback-report MIME 分段解析出的垃圾邮件投诉信息。
/// </summary>
public class IncomingMailFeedbackReport
    : SqlId,
        IEntityTypeConfiguration<IncomingMailFeedbackReport>
{
    private string? _originalRecipientEmail;

    /// <summary>产生该报告的分析运行。</summary>
    public long IncomingMailAnalysisId { get; set; }

    /// <summary>产生该报告的分析运行导航。</summary>
    public IncomingMailAnalysis IncomingMailAnalysis { get; set; } = null!;

    /// <summary>来源 feedback-report MIME 分段路径。</summary>
    public string MimePartPath { get; set; } = string.Empty;

    /// <summary>ARF 反馈类型。</summary>
    public IncomingMailFeedbackType FeedbackType { get; set; }

    /// <summary>ARF 中的原始收件人字段。</summary>
    public string? OriginalRecipient { get; set; }

    /// <summary>用于匹配本地收件人的规范化原始收件人邮箱。</summary>
    public string? OriginalRecipientEmail
    {
        get => _originalRecipientEmail;
        set => _originalRecipientEmail = value?.Trim().ToLowerInvariant();
    }

    /// <summary>ARF 报告的投诉域。</summary>
    public string? ReportedDomain { get; set; }

    /// <summary>ARF 报告的用户代理。</summary>
    public string? UserAgent { get; set; }

    /// <summary>配置一项分析中的投诉报告唯一性。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailFeedbackReport> builder)
    {
        builder.ToTable("IncomingMailFeedbackReports");
        builder.Property(x => x.MimePartPath).HasMaxLength(255).IsRequired();
        builder.Property(x => x.OriginalRecipient).HasMaxLength(1000);
        builder.Property(x => x.OriginalRecipientEmail).HasMaxLength(320);
        builder.Property(x => x.ReportedDomain).HasMaxLength(255);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.HasIndex(x => x.IncomingMailAnalysisId).IsUnique();
        builder.HasIndex(x => new { x.OriginalRecipientEmail, x.FeedbackType });
        builder
            .HasOne(x => x.IncomingMailAnalysis)
            .WithMany(x => x.FeedbackReports)
            .HasForeignKey(x => x.IncomingMailAnalysisId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
