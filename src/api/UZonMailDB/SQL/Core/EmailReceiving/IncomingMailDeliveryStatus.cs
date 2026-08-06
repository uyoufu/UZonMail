using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 从 delivery-status MIME 分段解析出的单个收件人投递结果。
/// 一封 DSN 可报告多个收件人，因此结果独立于邮件主表保存。
/// </summary>
public class IncomingMailDeliveryStatus
    : SqlId,
        IEntityTypeConfiguration<IncomingMailDeliveryStatus>
{
    private string? _finalRecipientEmail;

    /// <summary>产生该结果的分析运行。</summary>
    public long IncomingMailAnalysisId { get; set; }

    /// <summary>产生该结果的分析运行导航。</summary>
    public IncomingMailAnalysis IncomingMailAnalysis { get; set; } = null!;

    /// <summary>来源 delivery-status MIME 分段路径。</summary>
    public string MimePartPath { get; set; } = string.Empty;

    /// <summary>该分段中收件人块的顺序，从零开始。</summary>
    public int Position { get; set; }

    /// <summary>DSN 声明的投递动作。</summary>
    public IncomingMailDeliveryAction Action { get; set; }

    /// <summary>DSN 中的原始收件人字段。</summary>
    public string? OriginalRecipient { get; set; }

    /// <summary>DSN 中的最终收件人字段。</summary>
    public string? FinalRecipient { get; set; }

    /// <summary>用于匹配本地收件人的规范化最终邮箱地址。</summary>
    public string? FinalRecipientEmail
    {
        get => _finalRecipientEmail;
        set => _finalRecipientEmail = value?.Trim().ToLowerInvariant();
    }

    /// <summary>RFC 3463 增强状态码。</summary>
    public string? EnhancedStatusCode { get; set; }

    /// <summary>服务器报告的诊断代码。</summary>
    public string? DiagnosticCode { get; set; }

    /// <summary>服务器报告的远端 MTA。</summary>
    public string? RemoteMta { get; set; }

    /// <summary>配置 DSN 收件人块的唯一性和目标邮箱查询索引。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailDeliveryStatus> builder)
    {
        builder.ToTable("IncomingMailDeliveryStatuses");
        builder.Property(x => x.MimePartPath).HasMaxLength(255).IsRequired();
        builder.Property(x => x.OriginalRecipient).HasMaxLength(1000);
        builder.Property(x => x.FinalRecipient).HasMaxLength(1000);
        builder.Property(x => x.FinalRecipientEmail).HasMaxLength(320);
        builder.Property(x => x.EnhancedStatusCode).HasMaxLength(32);
        builder.Property(x => x.DiagnosticCode).HasMaxLength(2000);
        builder.Property(x => x.RemoteMta).HasMaxLength(1000);
        builder
            .HasIndex(x => new
            {
                x.IncomingMailAnalysisId,
                x.MimePartPath,
                x.Position
            })
            .IsUnique();
        builder.HasIndex(x => new { x.FinalRecipientEmail, x.Action });
        builder
            .HasOne(x => x.IncomingMailAnalysis)
            .WithMany(x => x.DeliveryStatuses)
            .HasForeignKey(x => x.IncomingMailAnalysisId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
