using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Files;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件的远端 MIME 分段清单及其可选本地缓存引用。
/// 分段元数据始终保留，文件仅在明确请求后下载。
/// </summary>
public class IncomingMailMimePart : SqlId, IEntityTypeConfiguration<IncomingMailMimePart>
{
    /// <summary>所属入站邮件。</summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>所属入站邮件导航。</summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>MIME 树中稳定的分段路径；原始 EML 使用空字符串。</summary>
    public string MimePartPath { get; set; } = string.Empty;

    /// <summary>父 MIME 分段路径；顶级分段为空。</summary>
    public string? ParentMimePartPath { get; set; }

    /// <summary>分段的业务用途。</summary>
    public IncomingMailMimePartKind PartKind { get; set; }

    /// <summary>服务器声明的内容处置方式。</summary>
    public IncomingMailContentDisposition ContentDisposition { get; set; }

    /// <summary>服务器声明的文件名。</summary>
    public string? FileName { get; set; }

    /// <summary>服务器声明的媒体类型。</summary>
    public string? MediaType { get; set; }

    /// <summary>HTML 内嵌资源的 Content-ID。</summary>
    public string? ContentId { get; set; }

    /// <summary>远端声明的分段字节数；未知时为空。</summary>
    public long? DeclaredSize { get; set; }

    /// <summary>分段本地缓存状态。</summary>
    public IncomingMailMimePartFetchStatus FetchStatus { get; set; }

    /// <summary>下载后创建的后台专属逻辑文件；未下载时为空。</summary>
    public long? FileUsageId { get; set; }

    /// <summary>下载后创建的后台专属逻辑文件导航。</summary>
    public FileUsage? FileUsage { get; set; }

    /// <summary>已下载内容的 SHA-256；未下载时为空。</summary>
    public string? ContentSha256 { get; set; }

    /// <summary>本地缓存文件的 UTC 下载时间。</summary>
    public DateTime? DownloadedAtUtc { get; set; }

    /// <summary>本地缓存的 UTC 过期时间。</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>最近一次下载失败的非敏感摘要。</summary>
    public string? LastFetchError { get; set; }

    /// <summary>配置远端分段清单、文件引用和清理查询索引。</summary>
    public void Configure(EntityTypeBuilder<IncomingMailMimePart> builder)
    {
        builder.ToTable("IncomingMailMimeParts");
        builder.Property(x => x.MimePartPath).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ParentMimePartPath).HasMaxLength(255);
        builder.Property(x => x.FileName).HasMaxLength(1000);
        builder.Property(x => x.MediaType).HasMaxLength(255);
        builder.Property(x => x.ContentId).HasMaxLength(1000);
        builder.Property(x => x.ContentSha256).HasMaxLength(64);
        builder.Property(x => x.LastFetchError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.IncomingMailMessageId, x.MimePartPath }).IsUnique();
        builder.HasIndex(x => new { x.FetchStatus, x.ExpiresAtUtc });
        builder.HasIndex(x => x.FileUsageId);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.MimeParts)
            .HasForeignKey(x => x.IncomingMailMessageId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.FileUsage)
            .WithMany()
            .HasForeignKey(x => x.FileUsageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
