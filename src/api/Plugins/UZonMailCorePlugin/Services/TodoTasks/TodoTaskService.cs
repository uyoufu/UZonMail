using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.TodoTasks.DTOs;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Todos;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.TodoTasks;

/// <summary>
/// 管理个人待办及邮件分支的来源快照。
/// </summary>
public sealed class TodoTaskService(SqlContext db) : IScopedService
{
    public async Task<List<TodoTaskDto>> GetAsync(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        var tasks = await BaseQuery(userId)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueAtUtc)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TodoTaskDto> GetAsync(
        long userId,
        long taskId,
        CancellationToken cancellationToken = default
    ) =>
        ToDto(
            await BaseQuery(userId).FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken)
                ?? throw new KnownException("待办不存在")
        );

    public async Task<TodoTaskDto> CreateAsync(
        long userId,
        long organizationId,
        UpsertTodoTaskRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var task = CreateTask(userId, organizationId, TodoTaskKind.Normal, request);
        db.TodoTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(task);
    }

    public async Task<TodoTaskDto> CreateMailAsync(
        long userId,
        long organizationId,
        CreateMailTodoTaskRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.SourceMessageId <= 0)
            throw new KnownException("请选择来源邮件");
        var sourceMessage = await db
            .MailConversationMessages.Where(x =>
                x.Id == request.SourceMessageId
                && x.MailConversationId == request.SourceConversationId
                && x.MailConversation.UserId == userId
            )
            .FirstOrDefaultAsync(cancellationToken);
        if (sourceMessage is null)
            throw new KnownException("来源邮件无效或不属于当前会话");
        var task = CreateTask(userId, organizationId, TodoTaskKind.MailFollowUp, request);
        var branch = new TodoMailBranch
        {
            TodoTask = task,
            SourceConversationId = request.SourceConversationId,
            BranchSubject = string.IsNullOrWhiteSpace(request.BranchSubject)
                ? task.Title
                : request.BranchSubject.Trim(),
            SourceMessages =
            [
                new TodoMailBranchSourceMessage
                {
                    MailConversationMessageId = sourceMessage.Id,
                },
            ],
        };
        task.MailBranch = branch;
        db.TodoTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(userId, task.Id, cancellationToken);
    }

    public async Task<TodoTaskDto> UpdateAsync(
        long userId,
        long taskId,
        UpsertTodoTaskRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var task =
            await db.TodoTasks.FirstOrDefaultAsync(
                x => x.Id == taskId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("待办不存在");
        ApplyRequest(task, request);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(userId, task.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        long userId,
        long taskId,
        CancellationToken cancellationToken = default
    )
    {
        var task =
            await BaseQuery(userId).FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken)
            ?? throw new KnownException("待办不存在");
        if (task.MailBranch is not null)
        {
            db.TodoMailBranchMessages.RemoveRange(task.MailBranch.Messages);
            db.TodoMailBranchSourceMessages.RemoveRange(task.MailBranch.SourceMessages);
            db.TodoMailBranches.Remove(task.MailBranch);
        }
        db.TodoTasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TodoTask> BaseQuery(long userId) =>
        db
            .TodoTasks.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.MailBranch)
            .ThenInclude(x => x!.SourceMessages)
            .ThenInclude(x => x.MailConversationMessage)
            .ThenInclude(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.Addresses)
            .Include(x => x.MailBranch)
            .ThenInclude(x => x!.SourceMessages)
            .ThenInclude(x => x.MailConversationMessage)
            .ThenInclude(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.MimeParts)
            .Include(x => x.MailBranch)
            .ThenInclude(x => x!.SourceMessages)
            .ThenInclude(x => x.MailConversationMessage)
            .ThenInclude(x => x.SendingItem)
            .ThenInclude(x => x!.Attachments!)
            .ThenInclude(x => x.FileObject)
            .Include(x => x.MailBranch)
            .ThenInclude(x => x!.Messages)
            .ThenInclude(x => x.MailConversationMessage)
            .ThenInclude(x => x.IncomingMailMessage)
            .ThenInclude(x => x!.Addresses)
            .Include(x => x.MailBranch)
            .ThenInclude(x => x!.Messages)
            .ThenInclude(x => x.MailConversationMessage)
            .ThenInclude(x => x.SendingItem)
            .AsSplitQuery();

    private static TodoTask CreateTask(
        long userId,
        long organizationId,
        TodoTaskKind kind,
        UpsertTodoTaskRequest request
    )
    {
        var task = new TodoTask
        {
            UserId = userId,
            OrganizationId = organizationId,
            Kind = kind
        };
        ApplyRequest(task, request);
        return task;
    }

    private static void ApplyRequest(TodoTask task, UpsertTodoTaskRequest request)
    {
        var title = request.Title.Trim();
        if (string.IsNullOrEmpty(title) || title.Length > 300)
            throw new KnownException("待办标题长度应为 1-300 个字符");
        task.Title = title;
        task.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueAtUtc = request.DueAtUtc;
        task.CompletedAtUtc =
            request.Status == TodoTaskStatus.Completed
                ? task.CompletedAtUtc ?? DateTime.UtcNow
                : null;
        task.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static TodoTaskDto ToDto(TodoTask task) =>
        new(
            task.Id,
            task.Kind,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueAtUtc,
            task.CompletedAtUtc,
            task.UpdatedAtUtc,
            task.MailBranch is null
                ? null
                : new TodoMailBranchDto(
                    task.MailBranch.Id,
                    task.MailBranch.SourceConversationId,
                    task.MailBranch.BranchSubject,
                    task.MailBranch.SourceMessages.OrderBy(x => x.MailConversationMessage.OccurredAtUtc)
                        .Select(x =>
                            MailConversationQueryService.ToMessageDto(x.MailConversationMessage)
                        )
                        .ToList(),
                    task.MailBranch.Messages.OrderBy(x => x.MailConversationMessage.OccurredAtUtc)
                        .Select(x =>
                            MailConversationQueryService.ToMessageDto(x.MailConversationMessage)
                        )
                        .ToList()
                )
        );
}
