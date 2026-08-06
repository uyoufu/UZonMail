namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// 邮件正文在本地的聚合可用状态。
/// MIME 分段的实际获取状态由 <see cref="IncomingMailMimePartFetchStatus"/> 记录。
/// </summary>
public enum IncomingMailBodyContentStatus
{
    /// <summary>
    /// 尚未请求正文。
    /// </summary>
    NotRequested = 0,

    /// <summary>
    /// 正文正在下载或保存。
    /// </summary>
    Downloading = 1,

    /// <summary>
    /// 至少有一个可渲染正文分段已保存。
    /// </summary>
    Available = 2,

    /// <summary>
    /// 最近一次正文下载失败。
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已保存的正文文件已按保留期清理。
    /// </summary>
    Expired = 4,
}

/// <summary>
/// MIME 分段的业务用途。
/// </summary>
public enum IncomingMailMimePartKind
{
    /// <summary>
    /// 原始 RFC 822 邮件。
    /// </summary>
    RawMessage = 0,

    /// <summary>
    /// HTML 正文。
    /// </summary>
    HtmlBody = 1,

    /// <summary>
    /// 纯文本正文。
    /// </summary>
    PlainTextBody = 2,

    /// <summary>
    /// 普通附件。
    /// </summary>
    Attachment = 3,

    /// <summary>
    /// HTML 正文引用的内嵌资源。
    /// </summary>
    InlineResource = 4,
}

/// <summary>
/// MIME 分段在本地文件缓存中的获取状态。
/// </summary>
public enum IncomingMailMimePartFetchStatus
{
    /// <summary>
    /// 已发现远端分段，但尚未下载。
    /// </summary>
    NotDownloaded = 0,

    /// <summary>
    /// 分段正在下载或保存。
    /// </summary>
    Downloading = 1,

    /// <summary>
    /// 分段文件已保存且可读取。
    /// </summary>
    Available = 2,

    /// <summary>
    /// 最近一次下载失败。
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已保存分段按保留期清理。
    /// </summary>
    Expired = 4,
}

/// <summary>
/// MIME Content-Disposition 的标准类型。
/// </summary>
public enum IncomingMailContentDisposition
{
    /// <summary>
    /// 未声明处置方式。
    /// </summary>
    None = 0,

    /// <summary>
    /// 作为普通附件处理。
    /// </summary>
    Attachment = 1,

    /// <summary>
    /// 作为正文内嵌资源处理。
    /// </summary>
    Inline = 2,
}

/// <summary>
/// 邮箱地址在邮件头中的角色。
/// </summary>
public enum IncomingMailAddressType
{
    /// <summary>
    /// 发件人地址。
    /// </summary>
    From = 0,

    /// <summary>
    /// 实际发送者地址。
    /// </summary>
    Sender = 1,

    /// <summary>
    /// 回复地址。
    /// </summary>
    ReplyTo = 2,

    /// <summary>
    /// 主收件人地址。
    /// </summary>
    To = 3,

    /// <summary>
    /// 抄送地址。
    /// </summary>
    Cc = 4,

    /// <summary>
    /// 密送地址。
    /// </summary>
    Bcc = 5,
}

/// <summary>
/// 邮件线程引用头的来源。
/// </summary>
public enum IncomingMailReferenceType
{
    /// <summary>
    /// In-Reply-To 头中的直接父消息标识。
    /// </summary>
    InReplyTo = 0,

    /// <summary>
    /// References 头中的线程祖先消息标识。
    /// </summary>
    References = 1,
}

/// <summary>
/// 由分析服务得出的单项入站邮件分类。
/// 多个同时生效的分类通过独立关联记录保存。
/// </summary>
public enum IncomingMailClassification
{
    /// <summary>
    /// 尚未判定任何业务分类。
    /// </summary>
    None = 0,

    /// <summary>
    /// 对已发送邮件的有效回复。
    /// </summary>
    Reply = 1,

    /// <summary>
    /// 永久无法投递的硬退信。
    /// </summary>
    HardBounce = 2,

    /// <summary>
    /// 暂时无法投递的软退信。
    /// </summary>
    SoftBounce = 3,

    /// <summary>
    /// 垃圾邮件或垃圾邮件投诉。
    /// </summary>
    Spam = 4,

    /// <summary>
    /// 自动回复或外出回复。
    /// </summary>
    AutoReply = 5,

    /// <summary>
    /// 退订请求。
    /// </summary>
    Unsubscribe = 6,
}

/// <summary>
/// 邮件列表中展示的主分类。
/// </summary>
public enum IncomingMailPrimaryClassification
{
    /// <summary>
    /// 尚未完成分类。
    /// </summary>
    None = 0,

    /// <summary>
    /// 有效回复。
    /// </summary>
    Reply = 1,

    /// <summary>
    /// 投递状态通知或退信。
    /// </summary>
    DeliveryStatus = 2,

    /// <summary>
    /// 垃圾邮件投诉或通知。
    /// </summary>
    Spam = 3,

    /// <summary>
    /// 自动回复。
    /// </summary>
    AutoReply = 4,

    /// <summary>
    /// 退订请求。
    /// </summary>
    Unsubscribe = 5,
}

/// <summary>
/// DSN 中报告的投递动作。
/// </summary>
public enum IncomingMailDeliveryAction
{
    /// <summary>
    /// 未识别的动作。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 已投递。
    /// </summary>
    Delivered = 1,

    /// <summary>
    /// 延迟投递。
    /// </summary>
    Delayed = 2,

    /// <summary>
    /// 投递失败。
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已转发。
    /// </summary>
    Relayed = 4,

    /// <summary>
    /// 已扩展到其他收件人。
    /// </summary>
    Expanded = 5,
}

/// <summary>
/// ARF 垃圾邮件反馈的标准类型。
/// </summary>
public enum IncomingMailFeedbackType
{
    /// <summary>
    /// 未识别的反馈类型。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 滥用或垃圾邮件投诉。
    /// </summary>
    Abuse = 1,

    /// <summary>
    /// 欺诈投诉。
    /// </summary>
    Fraud = 2,

    /// <summary>
    /// 病毒或恶意内容投诉。
    /// </summary>
    Virus = 3,

    /// <summary>
    /// 误判为垃圾邮件的反馈。
    /// </summary>
    NotSpam = 4,

    /// <summary>
    /// 认证失败反馈。
    /// </summary>
    AuthenticationFailure = 5,
}

/// <summary>
/// 分类证据的来源类型。
/// </summary>
public enum IncomingMailEvidenceType
{
    /// <summary>
    /// 邮件头字段。
    /// </summary>
    Header = 0,

    /// <summary>
    /// MIME 分段。
    /// </summary>
    MimePart = 1,

    /// <summary>
    /// DSN 收件人状态字段。
    /// </summary>
    DeliveryStatus = 2,

    /// <summary>
    /// ARF 反馈字段。
    /// </summary>
    FeedbackReport = 3,

    /// <summary>
    /// IMAP 文件夹或标记元数据。
    /// </summary>
    MailboxMetadata = 4,
}

/// <summary>
/// 退信的投递失败类型。
/// </summary>
public enum IncomingMailBounceType
{
    /// <summary>
    /// 未检测到退信。
    /// </summary>
    None = 0,

    /// <summary>
    /// 无法确定退信是否可恢复。
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// 收件地址永久不可投递。
    /// </summary>
    Hard = 2,

    /// <summary>
    /// 因配额、临时拒绝等原因导致的可重试退信。
    /// </summary>
    Soft = 3,
}

/// <summary>
/// 邮件分析结论的生成来源。
/// </summary>
public enum IncomingMailAnalysisSource
{
    /// <summary>
    /// 由确定性规则生成。
    /// </summary>
    RuleEngine = 0,

    /// <summary>
    /// 从 IMAP 文件夹或服务器标记直接得出。
    /// </summary>
    MailboxMetadata = 1,

    /// <summary>
    /// 由用户人工确认。
    /// </summary>
    Manual = 2,
}

/// <summary>
/// 单次邮件分析的执行状态。
/// </summary>
public enum IncomingMailAnalysisStatus
{
    /// <summary>
    /// 等待分析。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 正在分析。
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已成功产生结论。
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// 分析失败。
    /// </summary>
    Failed = 3,
}

/// <summary>
/// 入站邮件与发件项之间的业务关联类型。
/// </summary>
public enum IncomingMailLinkType
{
    /// <summary>
    /// 入站邮件是发件项的回复。
    /// </summary>
    Reply = 0,

    /// <summary>
    /// 入站邮件是发件项对应的投递状态通知或退信。
    /// </summary>
    DeliveryStatus = 1,

    /// <summary>
    /// 入站邮件是针对发件项的垃圾邮件投诉。
    /// </summary>
    SpamComplaint = 2,
}

/// <summary>
/// 入站邮件与发件项关联的判定依据。
/// </summary>
public enum IncomingMailLinkMatchMethod
{
    /// <summary>
    /// 通过匹配 RFC Message-ID 得出。
    /// </summary>
    InternetMessageId = 0,

    /// <summary>
    /// 通过 In-Reply-To 或 References 线程头得出。
    /// </summary>
    ThreadReference = 1,

    /// <summary>
    /// 通过地址、主题和时间窗口的弱规则得出。
    /// </summary>
    AddressTimeHeuristic = 2,

    /// <summary>
    /// 由用户人工指定。
    /// </summary>
    Manual = 3,
}

/// <summary>
/// 入站邮件与发件项关联的审核状态。
/// </summary>
public enum IncomingMailLinkStatus
{
    /// <summary>
    /// 自动规则提出、等待审核。
    /// </summary>
    Proposed = 0,

    /// <summary>
    /// 已被系统或用户确认。
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// 已被用户否决。
    /// </summary>
    Rejected = 2,
}

/// <summary>
/// 审计事件的操作者类型。
/// </summary>
public enum IncomingMailAuditActorType
{
    /// <summary>
    /// 后台系统或同步服务。
    /// </summary>
    System = 0,

    /// <summary>
    /// 已登录用户。
    /// </summary>
    User = 1,
}

/// <summary>
/// 收件领域需要保留的审计事件类型。
/// </summary>
public enum IncomingMailAuditEventType
{
    /// <summary>
    /// 发现新的远端邮件位置。
    /// </summary>
    MessageDiscovered = 0,

    /// <summary>
    /// 邮件内容文件已保存。
    /// </summary>
    ContentStored = 1,

    /// <summary>
    /// 邮件内容文件因保留期到达而清理。
    /// </summary>
    ContentExpired = 2,

    /// <summary>
    /// 远端文件夹中的邮件状态发生变化。
    /// </summary>
    LocationUpdated = 3,

    /// <summary>
    /// 已创建待回写的 IMAP 操作。
    /// </summary>
    CommandQueued = 4,

    /// <summary>
    /// 待回写的 IMAP 操作成功完成。
    /// </summary>
    CommandSucceeded = 5,

    /// <summary>
    /// 待回写的 IMAP 操作失败。
    /// </summary>
    CommandFailed = 6,

    /// <summary>
    /// 邮件分析已完成。
    /// </summary>
    AnalysisSucceeded = 7,

    /// <summary>
    /// 邮件分析失败。
    /// </summary>
    AnalysisFailed = 8,

    /// <summary>
    /// 邮件与发件项的关联被人工确认或否决。
    /// </summary>
    SendingItemLinkReviewed = 9,
}
