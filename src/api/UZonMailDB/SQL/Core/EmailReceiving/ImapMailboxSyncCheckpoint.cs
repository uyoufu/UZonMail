using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 文件夹已完整落库的增量同步检查点。
/// 服务器状态与本地提交进度分离，避免同步中断时跳过邮件。
/// </summary>
public class ImapMailboxSyncCheckpoint : SqlId, IEntityTypeConfiguration<ImapMailboxSyncCheckpoint>
{
    /// <summary>所属 IMAP 文件夹。</summary>
    public long ImapMailboxId { get; set; }

    /// <summary>所属 IMAP 文件夹导航。</summary>
    public ImapMailbox ImapMailbox { get; set; } = null!;

    /// <summary>当前检查点所属的 UID 命名空间。</summary>
    public long UidValidity { get; set; }

    /// <summary>已完整持久化的最大 UID；首次同步时为空。</summary>
    public long? LastCommittedUid { get; set; }

    /// <summary>已完整持久化的最大修改序列；服务器不支持时为空。</summary>
    public long? LastCommittedModSequence { get; set; }

    /// <summary>UIDVALIDITY 变化时递增的本地同步代次。</summary>
    public long SynchronizationGeneration { get; set; } = 1;

    /// <summary>最后一次完成全量存在性核对的 UTC 时间。</summary>
    public DateTime? LastFullReconciliationAtUtc { get; set; }

    /// <summary>检查点最近一次成功推进的 UTC 时间。</summary>
    public DateTime? LastCommittedAtUtc { get; set; }

    /// <summary>配置文件夹与检查点的一对一关系。</summary>
    public void Configure(EntityTypeBuilder<ImapMailboxSyncCheckpoint> builder)
    {
        builder.ToTable("ImapMailboxSyncCheckpoints");
        builder.HasIndex(x => x.ImapMailboxId).IsUnique();
        builder
            .HasOne(x => x.ImapMailbox)
            .WithOne(x => x.SyncCheckpoint)
            .HasForeignKey<ImapMailboxSyncCheckpoint>(x => x.ImapMailboxId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
