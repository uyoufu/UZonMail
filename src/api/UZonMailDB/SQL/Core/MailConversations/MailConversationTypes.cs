namespace UzonMail.DB.SQL.Core.MailConversations;

/// <summary>
/// 邮件会话的参与形式。
/// </summary>
public enum MailConversationType
{
    Direct = 0,
    Group = 1,
}

/// <summary>
/// 邮件相对于当前邮箱账号的方向。
/// </summary>
public enum MailMessageDirection
{
    Incoming = 0,
    Outgoing = 1,
}
