using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 记录需要可靠回写到 IMAP 服务器的本地邮件操作。
/// </summary>
public class ImapSyncCommand : SqlId, IEntityTypeConfiguration<ImapSyncCommand>
{
    /// <summary>
    /// 命令所属 IMAP 账户的标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 命令所属 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 命令操作的源文件夹标识。
    /// </summary>
    public long ImapMailboxId { get; set; }

    /// <summary>
    /// 命令操作的源文件夹。
    /// </summary>
    public ImapMailbox ImapMailbox { get; set; } = null!;

    /// <summary>
    /// 命令操作的邮件位置；清理文件夹等账户级操作时可为空。
    /// </summary>
    public long? IncomingMailLocationId { get; set; }

    /// <summary>
    /// 命令操作的邮件位置。
    /// </summary>
    public IncomingMailLocation? IncomingMailLocation { get; set; }

    /// <summary>
    /// 命令的业务类型。
    /// </summary>
    public ImapSyncCommandType CommandType { get; set; }

    /// <summary>
    /// 修改标记时采用的变更方式；非标记命令时为空。
    /// </summary>
    public ImapFlagMutationMode? FlagMutationMode { get; set; }

    /// <summary>
    /// 需要添加、删除或替换的系统标记。
    /// </summary>
    public ImapMessageFlags RequestedFlags { get; set; }

    /// <summary>
    /// 需要更新的自定义关键字；未操作关键字时为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 移动邮件时的目标文件夹标识；非移动命令时为空。
    /// </summary>
    public long? DestinationMailboxId { get; set; }

    /// <summary>
    /// 移动邮件时的目标文件夹。
    /// </summary>
    public ImapMailbox? DestinationMailbox { get; set; }

    /// <summary>
    /// 入队时目标位置所属的 UIDVALIDITY；账户级命令时为空。
    /// </summary>
    public long? ExpectedUidValidity { get; set; }

    /// <summary>
    /// 入队时目标位置的 UID；账户级命令时为空。
    /// </summary>
    public long? ExpectedUid { get; set; }

    /// <summary>
    /// 写入前期望的远端修改序列号，用于检测并发更新。
    /// </summary>
    public long? ExpectedModSequence { get; set; }

    /// <summary>
    /// 调用方生成的幂等键，避免同一远端操作重复入队。
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 当前命令执行状态。
    /// </summary>
    public ImapSyncCommandStatus Status { get; set; } = ImapSyncCommandStatus.Pending;

    /// <summary>
    /// 已执行的尝试次数。
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// 调度器下一次可尝试执行的 UTC 时间。
    /// </summary>
    public DateTime NextAttemptAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 命令完成执行的 UTC 时间；未完成时为空。
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 最近一次失败的无敏感信息摘要。
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 配置回写命令的调度索引与外键关系。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapSyncCommand> builder)
    {
        builder.ToTable("ImapSyncCommands");
        builder.Property(x => x.Keyword).HasMaxLength(255);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder.HasIndex(x => new { x.ImapAccountId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.ImapMailboxId, x.IncomingMailLocationId });
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany()
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapMailbox)
            .WithMany()
            .HasForeignKey(x => new { x.ImapMailboxId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.IncomingMailLocation)
            .WithMany()
            .HasForeignKey(x => new { x.IncomingMailLocationId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.DestinationMailbox)
            .WithMany()
            .HasForeignKey(x => new { x.DestinationMailboxId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
