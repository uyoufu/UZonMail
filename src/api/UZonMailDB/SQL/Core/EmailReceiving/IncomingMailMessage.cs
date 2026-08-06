using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 本地保留的入站邮件元数据。
/// 正文、EML 和附件通过 <see cref="IncomingMailMimePart"/> 的可选文件引用存储。
/// </summary>
public class IncomingMailMessage : SqlId, IEntityTypeConfiguration<IncomingMailMessage>
{
    private string? _internetMessageIdKey;

    /// <summary>
    /// 接收该邮件的 IMAP 账户标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 接收该邮件的 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 邮件头中的 RFC Message-ID；发件方未提供时为空。
    /// </summary>
    public string? InternetMessageId { get; set; }

    /// <summary>
    /// 规范化后的 RFC Message-ID，用于跨服务器格式差异下的精确匹配。
    /// </summary>
    public string? InternetMessageIdKey
    {
        get => _internetMessageIdKey;
        set => _internetMessageIdKey = value?.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 原始 EML 内容的 SHA-256；正文尚未下载时为空。
    /// </summary>
    public string? ContentSha256 { get; set; }

    /// <summary>
    /// 解码后的邮件主题，不保存正文内容。
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 邮件头 Date 表示的发送 UTC 时间；无法可靠转换时为空。
    /// </summary>
    public DateTime? SentAtUtc { get; set; }

    /// <summary>
    /// IMAP 服务器报告的邮件接收 UTC 时间。
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// 服务器报告的 RFC 822 邮件大小，单位为字节。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 邮件正文的聚合本地可用状态。
    /// </summary>
    public IncomingMailBodyContentStatus BodyContentStatus { get; set; }

    /// <summary>
    /// 已发现的普通附件数量，不代表附件已下载。
    /// </summary>
    public int AttachmentCount { get; set; }

    /// <summary>
    /// 已发现的 HTML 内嵌资源数量，不代表资源已下载。
    /// </summary>
    public int InlineResourceCount { get; set; }

    /// <summary>
    /// 邮件列表中展示的当前主分类。
    /// 其他同时生效的分类存储在 <see cref="CurrentClassifications"/>。
    /// </summary>
    public IncomingMailPrimaryClassification CurrentPrimaryClassification { get; set; }

    /// <summary>
    /// 当前生效的退信类型。
    /// </summary>
    public IncomingMailBounceType CurrentBounceType { get; set; }

    /// <summary>
    /// 当前生效的垃圾邮件评分；未获得评分时为空。
    /// </summary>
    public decimal? CurrentSpamScore { get; set; }

    /// <summary>
    /// 最近一次成功分析的 UTC 时间。
    /// </summary>
    public DateTime? LastAnalyzedAtUtc { get; set; }

    /// <summary>
    /// 当前分类最后更新的 UTC 时间。
    /// </summary>
    public DateTime? CurrentClassificationUpdatedAtUtc { get; set; }

    /// <summary>
    /// 当前分析结论的简短摘要，不包含邮件正文。
    /// </summary>
    public string? AnalysisSummary { get; set; }

    /// <summary>
    /// 邮件在各 IMAP 文件夹中的位置。
    /// </summary>
    public List<IncomingMailLocation> Locations { get; set; } = [];

    /// <summary>
    /// 邮件头中的结构化地址。
    /// </summary>
    public List<IncomingMailAddress> Addresses { get; set; } = [];

    /// <summary>
    /// 邮件线程引用头中的消息标识。
    /// </summary>
    public List<IncomingMailReference> References { get; set; } = [];

    /// <summary>
    /// 邮件远端 MIME 分段清单及其可选本地文件引用。
    /// </summary>
    public List<IncomingMailMimePart> MimeParts { get; set; } = [];

    /// <summary>
    /// 与发件项的归因关联。
    /// </summary>
    public List<IncomingMailSendingItemLink> SendingItemLinks { get; set; } = [];

    /// <summary>
    /// 可重跑的历史分析记录。
    /// </summary>
    public List<IncomingMailAnalysis> Analyses { get; set; } = [];

    /// <summary>
    /// 当前生效的多值分类集合。
    /// </summary>
    public List<IncomingMailCurrentClassification> CurrentClassifications { get; set; } = [];

    /// <summary>
    /// 配置消息内容指纹、时间线和分析查询索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailMessage> builder)
    {
        builder.ToTable("IncomingMailMessages");
        builder.Property(x => x.InternetMessageId).HasMaxLength(1000);
        builder.Property(x => x.InternetMessageIdKey).HasMaxLength(1000);
        builder.Property(x => x.ContentSha256).HasMaxLength(64);
        builder.Property(x => x.Subject).HasMaxLength(1000);
        builder.Property(x => x.AnalysisSummary).HasMaxLength(2000);
        builder.Property(x => x.CurrentSpamScore).HasPrecision(5, 2);
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder.HasIndex(x => new { x.ImapAccountId, x.InternetMessageIdKey });
        builder.HasIndex(x => new { x.ImapAccountId, x.ContentSha256 }).IsUnique();
        builder.HasIndex(x => new { x.ImapAccountId, x.ReceivedAtUtc });
        builder.HasIndex(x => new
        {
            x.ImapAccountId,
            x.CurrentPrimaryClassification,
            x.ReceivedAtUtc
        });
        builder.HasIndex(x => new
        {
            x.ImapAccountId,
            x.CurrentBounceType,
            x.ReceivedAtUtc
        });
        builder.HasIndex(x => new
        {
            x.ImapAccountId,
            x.BodyContentStatus,
            x.ReceivedAtUtc
        });
        builder.HasIndex(x => new { x.ImapAccountId, x.CurrentSpamScore });
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany()
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
