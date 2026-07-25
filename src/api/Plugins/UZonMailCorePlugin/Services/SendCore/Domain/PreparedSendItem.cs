using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.Domain;

/// <summary>
/// 已解析并可直接交给发送器使用的附件。
/// </summary>
public sealed record PreparedSendAttachment(string FileName, FileInfo File);

/// <summary>
/// 一次发送所需的完整、只读数据快照。
/// </summary>
public sealed record PreparedSendItem(
    SendingItem SourceItem,
    OutboxEmailAddress Outbox,
    SendingItemExcelData? Variables,
    string Subject,
    string HtmlBody,
    IReadOnlyList<PreparedSendAttachment> Attachments,
    IReadOnlyList<string> ReplyToEmails,
    IReadOnlyList<long> AvailableProxyIds,
    SendingSetting SendingSetting
)
{
    /// <summary>发件项所属用户 ID。</summary>
    public long UserId => SourceItem.UserId;

    /// <summary>主要收件人。</summary>
    public IReadOnlyList<EmailAddress> Inboxes => SourceItem.Inboxes;

    /// <summary>抄送收件人。</summary>
    public IReadOnlyList<EmailAddress> CC => SourceItem.CC ?? [];

    /// <summary>密送收件人。</summary>
    public IReadOnlyList<EmailAddress> BCC => SourceItem.BCC ?? [];

    /// <summary>当前用户设置的最大重试次数。</summary>
    public int MaxRetryCount => SendingSetting.MaxRetryCount;

    /// <summary>
    /// 邮件级代理优先于发件箱级代理。
    /// </summary>
    public long EffectiveProxyId => SourceItem.ProxyId > 0 ? SourceItem.ProxyId : Outbox.ProxyId;
}

/// <summary>
/// 已取得租约的一次发送执行。
/// </summary>
public sealed record SendItemExecution(SendLease Lease, PreparedSendItem PreparedItem)
{
    /// <summary>本次执行对应的轻量发件描述符。</summary>
    public SendItemDescriptor Descriptor => Lease.Item;
}
