using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 一次账户同步中某个 IMAP 文件夹的执行明细。
/// </summary>
public class ImapMailboxSyncRun : SqlId, IEntityTypeConfiguration<ImapMailboxSyncRun>
{
    /// <summary>
    /// 本次同步所属 IMAP 账户的标识。
    /// 与同步批次、文件夹组成复合外键，禁止跨账户记录。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 所属账户同步记录的标识。
    /// </summary>
    public long ImapSyncRunId { get; set; }

    /// <summary>
    /// 所属账户同步记录。
    /// </summary>
    public ImapSyncRun ImapSyncRun { get; set; } = null!;

    /// <summary>
    /// 本次处理的 IMAP 文件夹标识。
    /// </summary>
    public long ImapMailboxId { get; set; }

    /// <summary>
    /// 本次处理的 IMAP 文件夹。
    /// </summary>
    public ImapMailbox ImapMailbox { get; set; } = null!;

    /// <summary>
    /// 文件夹同步的执行状态。
    /// </summary>
    public ImapSyncStatus Status { get; set; }

    /// <summary>
    /// 本次查询的起始 UID；首次全量同步时为空。
    /// </summary>
    public long? StartUid { get; set; }

    /// <summary>
    /// 本次查询的结束 UID；未发现邮件时为空。
    /// </summary>
    public long? EndUid { get; set; }

    /// <summary>
    /// 本次文件夹同步开始时的已提交修改序列；服务器不支持时为空。
    /// </summary>
    public long? StartCommittedModSequence { get; set; }

    /// <summary>
    /// 本次文件夹同步完成后的已提交修改序列；未推进时为空。
    /// </summary>
    public long? EndCommittedModSequence { get; set; }

    /// <summary>
    /// 本次执行是否推进了持久化同步检查点。
    /// </summary>
    public bool CheckpointAdvanced { get; set; }

    /// <summary>
    /// 从该文件夹发现的邮件数量。
    /// </summary>
    public int MessagesDiscovered { get; set; }

    /// <summary>
    /// 为该文件夹新建的位置记录数量。
    /// </summary>
    public int LocationsCreated { get; set; }

    /// <summary>
    /// 更新的位置记录数量。
    /// </summary>
    public int LocationsUpdated { get; set; }

    /// <summary>
    /// 从该文件夹下载的文件总字节数。
    /// </summary>
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 此文件夹处理失败的摘要。
    /// </summary>
    public string? ErrorSummary { get; set; }

    /// <summary>
    /// 配置一次同步内文件夹记录的唯一性和关系。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapMailboxSyncRun> builder)
    {
        builder.ToTable("ImapMailboxSyncRuns");
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ImapSyncRunId, x.ImapMailboxId }).IsUnique();
        builder
            .HasOne(x => x.ImapSyncRun)
            .WithMany(x => x.MailboxRuns)
            .HasForeignKey(x => new { x.ImapSyncRunId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapMailbox)
            .WithMany()
            .HasForeignKey(x => new { x.ImapMailboxId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
