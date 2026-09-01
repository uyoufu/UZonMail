namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 将协议同步得到的邮箱消息投影为用户可操作的稳定会话。
/// </summary>
public interface IMailConversationIngestionService
{
    Task IngestAsync(long mailboxMessageId, CancellationToken cancellationToken = default);
}
