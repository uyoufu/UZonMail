using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 服务器上的文件夹及其增量同步水位。
/// </summary>
public class ImapMailbox : SqlId, IEntityTypeConfiguration<ImapMailbox>
{
    /// <summary>
    /// 所属 IMAP 账户的数据库标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 所属 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 父文件夹标识；根文件夹为空。
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 父文件夹。
    /// </summary>
    public ImapMailbox? Parent { get; set; }

    /// <summary>
    /// 子文件夹集合。
    /// </summary>
    public List<ImapMailbox> Children { get; set; } = [];

    /// <summary>
    /// 文件夹已完整持久化的增量同步检查点。
    /// </summary>
    public ImapMailboxSyncCheckpoint? SyncCheckpoint { get; set; }

    /// <summary>
    /// 服务器返回的完整文件夹名称，用于 IMAP 命令寻址。
    /// </summary>
    public string RemoteFullName { get; set; } = string.Empty;

    /// <summary>
    /// 展示给用户的文件夹名称。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 服务器声明的层级分隔符；根文件夹或未声明时为空。
    /// </summary>
    public string? HierarchyDelimiter { get; set; }

    /// <summary>
    /// 文件夹的标准特殊用途。
    /// </summary>
    public ImapMailboxSpecialUse SpecialUse { get; set; }

    /// <summary>
    /// 服务器报告的文件夹属性。
    /// </summary>
    public ImapMailboxAttributes Attributes { get; set; }

    /// <summary>
    /// 文件夹是否在服务器订阅列表中。
    /// </summary>
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// 是否将该文件夹纳入后台同步。
    /// </summary>
    public bool IsSynchronizationEnabled { get; set; }

    /// <summary>
    /// 当前 UID 命名空间的有效性值；变化时必须重新建立该文件夹的本地位置映射。
    /// </summary>
    public long? UidValidity { get; set; }

    /// <summary>
    /// 服务器报告的下一个可分配 UID。
    /// </summary>
    public long? UidNext { get; set; }

    /// <summary>
    /// 服务器在支持 CONDSTORE 时报告的最高修改序列号。
    /// </summary>
    public long? HighestModSequence { get; set; }

    /// <summary>
    /// 最近一次同步时服务器报告的邮件数量。
    /// </summary>
    public int RemoteMessageCount { get; set; }

    /// <summary>
    /// 最近一次同步时服务器报告的未读邮件数量。
    /// </summary>
    public int RemoteUnreadCount { get; set; }

    /// <summary>
    /// 最近一次成功同步该文件夹的 UTC 时间。
    /// </summary>
    public DateTime? LastSuccessfulSyncAtUtc { get; set; }

    /// <summary>
    /// 最近一次同步该文件夹的状态。
    /// </summary>
    public ImapSyncStatus LastSyncStatus { get; set; }

    /// <summary>
    /// 最近一次同步失败的摘要。
    /// </summary>
    public string? LastSyncError { get; set; }

    /// <summary>
    /// 当前位于此文件夹中的邮件位置记录。
    /// </summary>
    public List<IncomingMailLocation> MessageLocations { get; set; } = [];

    /// <summary>
    /// 配置文件夹树、同步水位和远端名称唯一性。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapMailbox> builder)
    {
        builder.ToTable("ImapMailboxes");
        builder.Property(x => x.RemoteFullName).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.HierarchyDelimiter).HasMaxLength(1);
        builder.Property(x => x.LastSyncError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ImapAccountId, x.RemoteFullName }).IsUnique();
        builder.HasIndex(x => new { x.ImapAccountId, x.IsSynchronizationEnabled });
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder
            .HasOne(x => x.ImapAccount)
            .WithMany(x => x.Mailboxes)
            .HasForeignKey(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => new { x.ParentId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
