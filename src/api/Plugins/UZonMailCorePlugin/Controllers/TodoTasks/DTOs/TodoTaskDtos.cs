using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.DB.SQL.Core.Todos;

namespace UzonMail.CorePlugin.Controllers.TodoTasks.DTOs;

public sealed record TodoTaskDto(
    long Id,
    TodoTaskKind Kind,
    string Title,
    string? Description,
    TodoTaskStatus Status,
    TodoTaskPriority Priority,
    DateTime? DueAtUtc,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc,
    TodoMailBranchDto? MailBranch
);

public sealed record TodoMailBranchDto(
    long Id,
    long SourceConversationId,
    string BranchSubject,
    IReadOnlyList<MailConversationMessageDto> SourceMessages,
    IReadOnlyList<MailConversationMessageDto> Messages
);

public class UpsertTodoTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TodoTaskStatus Status { get; set; }
    public TodoTaskPriority Priority { get; set; } = TodoTaskPriority.Normal;
    public DateTime? DueAtUtc { get; set; }
}

public sealed class CreateMailTodoTaskRequest : UpsertTodoTaskRequest
{
    public long SourceConversationId { get; set; }
    public long SourceMessageId { get; set; }
    public string? BranchSubject { get; set; }
}
