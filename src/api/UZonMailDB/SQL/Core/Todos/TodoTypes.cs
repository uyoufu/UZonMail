namespace UzonMail.DB.SQL.Core.Todos;

public enum TodoTaskKind
{
    Normal = 0,
    MailFollowUp = 1,
}

public enum TodoTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Archived = 3,
}

public enum TodoTaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3,
}
