using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Controllers.TodoTasks.DTOs;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.TodoTasks;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.TodoTasks;

/// <summary>
/// 个人普通待办及从邮件会话派生的跟进分支。
/// </summary>
public sealed class TodoTasksController(
    TokenService tokenService,
    TodoTaskService taskService,
    MailConversationSendService sendService
) : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<TodoTaskDto>>> Get(CancellationToken cancellationToken) =>
        (
            await taskService.GetAsync(tokenService.GetUserSqlId(), cancellationToken)
        ).ToSuccessResponse();

    [HttpGet("{taskId:long}")]
    public async Task<ResponseResult<TodoTaskDto>> Get(
        long taskId,
        CancellationToken cancellationToken
    ) =>
        (
            await taskService.GetAsync(tokenService.GetUserSqlId(), taskId, cancellationToken)
        ).ToSuccessResponse();

    [HttpPost]
    public async Task<ResponseResult<TodoTaskDto>> Create(
        [FromBody] UpsertTodoTaskRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await taskService.CreateAsync(
                tokenService.GetUserSqlId(),
                tokenService.GetOrganizationId(),
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpPost("mail")]
    public async Task<ResponseResult<TodoTaskDto>> CreateMail(
        [FromBody] CreateMailTodoTaskRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await taskService.CreateMailAsync(
                tokenService.GetUserSqlId(),
                tokenService.GetOrganizationId(),
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpPut("{taskId:long}")]
    public async Task<ResponseResult<TodoTaskDto>> Update(
        long taskId,
        [FromBody] UpsertTodoTaskRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await taskService.UpdateAsync(
                tokenService.GetUserSqlId(),
                taskId,
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpDelete("{taskId:long}")]
    public async Task<ResponseResult<bool>> Delete(long taskId, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(tokenService.GetUserSqlId(), taskId, cancellationToken);
        return true.ToSuccessResponse();
    }

    [HttpPost("{taskId:long}/messages")]
    public async Task<ResponseResult<SendConversationMessageResult>> SendMail(
        long taskId,
        [FromBody] SendConversationMessageRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await sendService.SendTodoBranchAsync(
                tokenService.GetUserSqlId(),
                taskId,
                request,
                cancellationToken
            )
        ).ToSuccessResponse();
}
