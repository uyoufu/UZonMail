namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 文件夹的标准特殊用途。
/// </summary>
public enum ImapMailboxSpecialUse
{
    /// <summary>
    /// 服务器未声明特殊用途。
    /// </summary>
    None = 0,

    /// <summary>
    /// 收件箱。
    /// </summary>
    RecipientContact = 1,

    /// <summary>
    /// 已发送邮件。
    /// </summary>
    Sent = 2,

    /// <summary>
    /// 草稿箱。
    /// </summary>
    Drafts = 3,

    /// <summary>
    /// 已删除邮件箱。
    /// </summary>
    Trash = 4,

    /// <summary>
    /// 垃圾邮件箱。
    /// </summary>
    Junk = 5,

    /// <summary>
    /// 归档箱。
    /// </summary>
    Archive = 6,

    /// <summary>
    /// 服务商提供的全部邮件视图。
    /// </summary>
    All = 7,
}

/// <summary>
/// IMAP 服务器返回的文件夹属性。
/// </summary>
[Flags]
public enum ImapMailboxAttributes
{
    /// <summary>
    /// 未返回额外属性。
    /// </summary>
    None = 0,

    /// <summary>
    /// 文件夹不能被选择或读取邮件。
    /// </summary>
    NoSelect = 1 << 0,

    /// <summary>
    /// 文件夹在服务器上不存在。
    /// </summary>
    NonExistent = 1 << 1,

    /// <summary>
    /// 文件夹已被服务器标记为有变化。
    /// </summary>
    Marked = 1 << 2,

    /// <summary>
    /// 文件夹被服务器标记为无变化。
    /// </summary>
    Unmarked = 1 << 3,

    /// <summary>
    /// 文件夹包含子文件夹。
    /// </summary>
    HasChildren = 1 << 4,

    /// <summary>
    /// 文件夹不包含子文件夹。
    /// </summary>
    HasNoChildren = 1 << 5,

    /// <summary>
    /// 文件夹订阅状态由服务器确认。
    /// </summary>
    Subscribed = 1 << 6,
}

/// <summary>
/// 同步任务或文件夹同步的执行状态。
/// </summary>
public enum ImapSyncStatus
{
    /// <summary>
    /// 尚未开始执行。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 正在执行。
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已成功完成。
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// 执行失败。
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 执行被取消。
    /// </summary>
    Cancelled = 4,
}

/// <summary>
/// 触发 IMAP 同步的来源。
/// </summary>
public enum ImapSyncTrigger
{
    /// <summary>
    /// 由后台定时调度触发。
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// 由用户手动触发。
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 由本地邮件操作触发的补偿同步。
    /// </summary>
    LocalChange = 2,
}

/// <summary>
/// 需要回写到 IMAP 服务器的操作类型。
/// </summary>
public enum ImapSyncCommandType
{
    /// <summary>
    /// 修改系统标记或自定义关键字。
    /// </summary>
    UpdateFlags = 0,

    /// <summary>
    /// 将邮件移动到目标文件夹。
    /// </summary>
    MoveMessage = 1,

    /// <summary>
    /// 标记邮件为已删除。
    /// </summary>
    MarkDeleted = 2,

    /// <summary>
    /// 从服务器永久清除已删除邮件。
    /// </summary>
    Expunge = 3,
}

/// <summary>
/// IMAP 回写命令的执行状态。
/// </summary>
public enum ImapSyncCommandStatus
{
    /// <summary>
    /// 等待执行。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 正在执行。
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已成功回写到服务器。
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// 可在稍后重试的失败。
    /// </summary>
    RetryableFailed = 3,

    /// <summary>
    /// 不可重试的终止失败。
    /// </summary>
    Failed = 4,
}

/// <summary>
/// IMAP 邮件的标准系统标记。
/// </summary>
[Flags]
public enum ImapMessageFlags
{
    /// <summary>
    /// 没有系统标记。
    /// </summary>
    None = 0,

    /// <summary>
    /// 邮件已读。
    /// </summary>
    Seen = 1 << 0,

    /// <summary>
    /// 邮件已回复。
    /// </summary>
    Answered = 1 << 1,

    /// <summary>
    /// 邮件已加星标或标记。
    /// </summary>
    Flagged = 1 << 2,

    /// <summary>
    /// 邮件是草稿。
    /// </summary>
    Draft = 1 << 3,

    /// <summary>
    /// 邮件已标记删除。
    /// </summary>
    Deleted = 1 << 4,

    /// <summary>
    /// 邮件自上次选择文件夹后新到达。
    /// </summary>
    Recent = 1 << 5,
}

/// <summary>
/// 修改 IMAP 标记的方式。
/// </summary>
public enum ImapFlagMutationMode
{
    /// <summary>
    /// 向当前标记集合增加值。
    /// </summary>
    Add = 0,

    /// <summary>
    /// 从当前标记集合移除值。
    /// </summary>
    Remove = 1,

    /// <summary>
    /// 以指定标记集合完全替换当前值。
    /// </summary>
    Replace = 2,
}
