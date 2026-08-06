using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 用户配置的独立 IMAP 收件账户。
/// 与 SMTP 发件箱解耦，以支持集中收取多个发件地址的回信。
/// </summary>
public class ImapAccount : UserAndOrgId, IEntityTypeConfiguration<ImapAccount>
{
    private string _email = string.Empty;

    /// <summary>
    /// 用户在系统中识别该收件账户的名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 用于收信的邮箱地址；写入时统一去除空格并转为小写。
    /// </summary>
    public string Email
    {
        get => _email;
        set => _email = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// IMAP 服务器主机名。
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// IMAP 服务器端口。
    /// </summary>
    public int Port { get; set; } = 993;

    /// <summary>
    /// 连接 IMAP 服务器时使用的安全套接字策略。
    /// </summary>
    public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;

    /// <summary>
    /// 当前账户使用的认证方式。
    /// </summary>
    public ImapAuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// 账户是否允许后台同步。
    /// </summary>
    public ImapAccountStatus Status { get; set; } = ImapAccountStatus.Active;

    /// <summary>
    /// 邮件内容文件的保留天数；到期后仅保留邮件元数据和分析结果。
    /// </summary>
    public int ContentRetentionDays { get; set; } = 30;

    /// <summary>
    /// 最近一次完整成功同步的 UTC 时间。
    /// </summary>
    public DateTime? LastSuccessfulSyncAtUtc { get; set; }

    /// <summary>
    /// 最近一次尝试同步的 UTC 时间。
    /// </summary>
    public DateTime? LastSyncAttemptAtUtc { get; set; }

    /// <summary>
    /// 后台调度器下一次可尝试同步的 UTC 时间。
    /// </summary>
    public DateTime? NextSyncAtUtc { get; set; }

    /// <summary>
    /// 最近一次成功连接服务器的 UTC 时间。
    /// </summary>
    public DateTime? LastConnectedAtUtc { get; set; }

    /// <summary>
    /// 最近一次同步或连接失败的摘要，不保存凭据或邮件正文。
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 账户的认证凭据。
    /// </summary>
    public ImapAccountCredential Credential { get; set; } = null!;

    /// <summary>
    /// 此收件账户可归因的 SMTP 发件箱。
    /// </summary>
    public List<ImapAccountOutboxLink> OutboxLinks { get; set; } = [];

    /// <summary>
    /// 地址弱匹配时优先归因的发件箱关联；未指定时为空。
    /// </summary>
    public ImapAccountPrimaryOutbox? PrimaryOutbox { get; set; }

    /// <summary>
    /// 从服务器发现的 IMAP 文件夹。
    /// </summary>
    public List<ImapMailbox> Mailboxes { get; set; } = [];

    /// <summary>
    /// 配置收件账户的唯一性和字段长度。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapAccount> builder)
    {
        builder.ToTable("ImapAccounts");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Host).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder
            .HasIndex(x => new
            {
                x.UserId,
                x.Email,
                x.Host,
                x.Port,
            })
            .IsUnique()
            .HasDatabaseName("IX_ImapAccounts_User_Email_Host_Port");
        builder.HasIndex(x => new { x.Status, x.NextSyncAtUtc });
    }
}
