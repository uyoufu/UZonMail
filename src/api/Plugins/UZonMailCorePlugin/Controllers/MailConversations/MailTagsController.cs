using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.MailConversations;

/// <summary>
/// 用户级邮件联系人标签。
/// </summary>
public sealed class MailTagsController(TokenService tokenService, MailTagService tagService)
    : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<MailTagDto>>> Get(CancellationToken cancellationToken) =>
        (
            await tagService.GetAsync(tokenService.GetUserSqlId(), cancellationToken)
        ).ToSuccessResponse();

    [HttpPost]
    public async Task<ResponseResult<MailTagDto>> Create(
        [FromBody] UpsertMailTagRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await tagService.CreateAsync(
                tokenService.GetUserSqlId(),
                tokenService.GetOrganizationId(),
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpPut("{tagId:long}")]
    public async Task<ResponseResult<MailTagDto>> Update(
        long tagId,
        [FromBody] UpsertMailTagRequest request,
        CancellationToken cancellationToken
    ) =>
        (
            await tagService.UpdateAsync(
                tokenService.GetUserSqlId(),
                tagId,
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpDelete("{tagId:long}")]
    public async Task<ResponseResult<bool>> Delete(long tagId, CancellationToken cancellationToken)
    {
        await tagService.DeleteAsync(tokenService.GetUserSqlId(), tagId, cancellationToken);
        return true.ToSuccessResponse();
    }

    [HttpPut("contacts/{contactId:long}")]
    public async Task<ResponseResult<bool>> SetContactTags(
        long contactId,
        [FromBody] SetMailContactTagsRequest request,
        CancellationToken cancellationToken
    )
    {
        await tagService.SetContactTagsAsync(
            tokenService.GetUserSqlId(),
            contactId,
            request.TagIds,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }
}
