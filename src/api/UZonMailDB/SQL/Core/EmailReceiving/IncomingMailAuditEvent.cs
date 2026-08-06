using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 收件同步、分析和人工操作的审计事件。
/// </summary>
public class IncomingMailAuditEvent : SqlId, IEntityTypeConfiguration<IncomingMailAuditEvent>
{
    /// <summary>
    /// 审计事件所属 IMAP 账户的标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 审计事件所属 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 事件关联的入站邮件标识；账户级事件时为空。
    /// </summary>
    public long? IncomingMailMessageId { get; set; }

    /// <summary>
    /// 事件关联的入站邮件。
    /// </summary>
    public IncomingMailMessage? IncomingMailMessage { get; set; }

    /// <summary>
    /// 事件关联的邮件位置标识；非位置事件时为空。
    /// </summary>
    public long? IncomingMailLocationId { get; set; }

    /// <summary>
    /// 事件关联的邮件位置。
    /// </summary>
    public IncomingMailLocation? IncomingMailLocation { get; set; }

    /// <summary>
    /// 事件关联的同步批次标识；非同步事件时为空。
    /// </summary>
    public long? ImapSyncRunId { get; set; }

    /// <summary>
    /// 事件关联的同步批次。
    /// </summary>
    public ImapSyncRun? ImapSyncRun { get; set; }

    /// <summary>
    /// 事件关联的回写命令标识；非回写事件时为空。
    /// </summary>
    public long? ImapSyncCommandId { get; set; }

    /// <summary>
    /// 事件关联的回写命令。
    /// </summary>
    public ImapSyncCommand? ImapSyncCommand { get; set; }

    /// <summary>
    /// 事件由后台系统还是用户产生。
    /// </summary>
    public IncomingMailAuditActorType ActorType { get; set; }

    /// <summary>
    /// 产生事件的用户标识；系统事件时为空。
    /// </summary>
    public long? ActorUserId { get; set; }

    /// <summary>
    /// 审计事件的标准类型。
    /// </summary>
    public IncomingMailAuditEventType EventType { get; set; }

    /// <summary>
    /// 事件实际发生的 UTC 时间。
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// 不包含凭据或邮件正文的事件说明。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 配置审计时间线的关系和查询索引。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailAuditEvent> builder)
    {
        builder.ToTable("IncomingMailAuditEvents");
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ImapAccountId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.IncomingMailMessageId, x.OccurredAtUtc });
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany()
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany()
            .HasForeignKey(x => new { x.IncomingMailMessageId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.IncomingMailLocation)
            .WithMany()
            .HasForeignKey(x => new { x.IncomingMailLocationId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapSyncRun)
            .WithMany()
            .HasForeignKey(x => new { x.ImapSyncRunId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapSyncCommand)
            .WithMany()
            .HasForeignKey(x => new { x.ImapSyncCommandId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
