using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 入站邮件在一个 IMAP 文件夹中的远端位置与状态。
/// </summary>
public class IncomingMailLocation : SqlId, IEntityTypeConfiguration<IncomingMailLocation>
{
    /// <summary>
    /// 所属 IMAP 账户的标识。
    /// 与邮件和文件夹组成复合外键，禁止跨账户位置关联。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 所属入站邮件元数据的标识。
    /// </summary>
    public long IncomingMailMessageId { get; set; }

    /// <summary>
    /// 所属入站邮件元数据。
    /// </summary>
    public IncomingMailMessage IncomingMailMessage { get; set; } = null!;

    /// <summary>
    /// 邮件所在 IMAP 文件夹的标识。
    /// </summary>
    public long ImapMailboxId { get; set; }

    /// <summary>
    /// 邮件所在 IMAP 文件夹。
    /// </summary>
    public ImapMailbox ImapMailbox { get; set; } = null!;

    /// <summary>
    /// 创建该 UID 的远端 UIDVALIDITY 值。
    /// </summary>
    public long UidValidity { get; set; }

    /// <summary>
    /// 邮件在此文件夹 UIDVALIDITY 范围内的稳定 UID。
    /// </summary>
    public long Uid { get; set; }

    /// <summary>
    /// 服务器支持 CONDSTORE 时的远端修改序列号。
    /// </summary>
    public long? ModSequence { get; set; }

    /// <summary>
    /// 此文件夹中邮件的 IMAP 系统标记。
    /// </summary>
    public ImapMessageFlags Flags { get; set; }

    /// <summary>
    /// 邮件是否仍存在于服务器的此文件夹中。
    /// </summary>
    public bool IsPresentOnServer { get; set; } = true;

    /// <summary>
    /// 首次在此文件夹发现邮件的 UTC 时间。
    /// </summary>
    public DateTime FirstSeenAtUtc { get; set; }

    /// <summary>
    /// 最近一次同步此位置的 UTC 时间。
    /// </summary>
    public DateTime LastSynchronizedAtUtc { get; set; }

    /// <summary>
    /// 此位置上附加的服务器自定义关键字。
    /// </summary>
    public List<IncomingMailKeyword> Keywords { get; set; } = [];

    /// <summary>
    /// 配置远端位置的 UID 唯一性和关系。
    /// </summary>
    public void Configure(EntityTypeBuilder<IncomingMailLocation> builder)
    {
        builder.ToTable("IncomingMailLocations");
        builder
            .HasIndex(x => new
            {
                x.ImapMailboxId,
                x.UidValidity,
                x.Uid
            })
            .IsUnique();
        builder.HasAlternateKey(x => new { x.Id, x.ImapAccountId });
        builder.HasIndex(x => new
        {
            x.ImapMailboxId,
            x.IsPresentOnServer,
            x.Uid
        });
        builder.HasIndex(x => new { x.ImapMailboxId, x.ModSequence });
        builder
            .HasOne(x => x.IncomingMailMessage)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => new { x.IncomingMailMessageId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasOne(x => x.ImapMailbox)
            .WithMany(x => x.MessageLocations)
            .HasForeignKey(x => new { x.ImapMailboxId, x.ImapAccountId })
            .HasPrincipalKey(x => new { x.Id, x.ImapAccountId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
