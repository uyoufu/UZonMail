using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 一次 IMAP 账户同步的审计记录。
/// </summary>
public class ImapSyncRun : SqlId, IEntityTypeConfiguration<ImapSyncRun>
{
    /// <summary>
    /// 执行同步的 IMAP 账户标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 执行同步的 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 本次同步的触发来源。
    /// </summary>
    public ImapSyncTrigger Trigger { get; set; }

    /// <summary>
    /// 本次同步的执行状态。
    /// </summary>
    public ImapSyncStatus Status { get; set; }

    /// <summary>
    /// 开始执行的 UTC 时间。
    /// </summary>
    public DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// 完成执行的 UTC 时间；未结束时为空。
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 本次尝试同步的文件夹数量。
    /// </summary>
    public int MailboxesAttempted { get; set; }

    /// <summary>
    /// 从服务器发现的邮件数量。
    /// </summary>
    public int MessagesDiscovered { get; set; }

    /// <summary>
    /// 新建的本地邮件记录数量。
    /// </summary>
    public int MessagesCreated { get; set; }

    /// <summary>
    /// 更新的本地邮件记录数量。
    /// </summary>
    public int MessagesUpdated { get; set; }

    /// <summary>
    /// 标记为远端已移除的邮件位置数量。
    /// </summary>
    public int MessageLocationsRemoved { get; set; }

    /// <summary>
    /// 本次从服务器下载的文件总字节数。
    /// </summary>
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 失败时记录的无敏感信息摘要。
    /// </summary>
    public string? ErrorSummary { get; set; }

    /// <summary>
    /// 本次同步中各文件夹的执行明细。
    /// </summary>
    public List<ImapMailboxSyncRun> MailboxRuns { get; set; } = [];

    /// <summary>
    /// 配置账户同步审计的查询索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapSyncRun> builder)
    {
        builder.ToTable("ImapSyncRuns");
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000);
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder.HasIndex(x => new { x.ImapAccountId, x.StartedAtUtc });
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany()
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
