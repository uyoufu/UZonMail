using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails;

/// <summary>
/// 邮箱账户与收件邮箱共用的平面分组接口。
/// </summary>
public sealed class EmailGroupController(
    SqlContext db,
    EmailGroupService groupService,
    TokenService tokenService
) : ControllerBaseV1
{
    [HttpGet("{emailGroupId:long}")]
    public async Task<ResponseResult<EmailGroupSummaryDto>> FindOneById(
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var userId = tokenService.GetUserSqlId();
        var group =
            await db.EmailGroups.FirstOrDefaultAsync(
                x => x.Id == emailGroupId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮箱分组不存在");
        return (await ToSummaryAsync(group, cancellationToken)).ToSuccessResponse();
    }

    [HttpPost]
    public async Task<ResponseResult<EmailGroupSummaryDto>> Create(
        [FromBody] CreateEmailGroupDto request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateCategory(request.Category);
        var group = await groupService.Create(
            new EmailGroup
            {
                UserId = tokenService.GetUserSqlId(),
                Category = request.Category,
                Icon = request.Icon,
                Name = request.Name,
                Description = request.Description,
            }
        );
        return (await ToSummaryAsync(group, cancellationToken)).ToSuccessResponse();
    }

    [HttpGet("all")]
    public async Task<ResponseResult<List<EmailGroupSummaryDto>>> GetEmailGroups(
        [FromQuery] EmailGroupCategory category,
        CancellationToken cancellationToken = default
    )
    {
        ValidateCategory(category);
        var groups = await groupService.GetEmailGroupsAsync(
            tokenService.GetUserSqlId(),
            category,
            cancellationToken
        );
        return (await ToSummariesAsync(groups, cancellationToken)).ToSuccessResponse();
    }

    [HttpPut("{emailGroupId:long}")]
    public async Task<ResponseResult<EmailGroupSummaryDto>> Update(
        long emailGroupId,
        [FromBody] UpdateEmailGroupDto request,
        CancellationToken cancellationToken = default
    )
    {
        var group = await groupService.UpdateMetadataAsync(
            tokenService.GetUserSqlId(),
            emailGroupId,
            request.Name,
            request.Description,
            cancellationToken
        );
        return (await ToSummaryAsync(group, cancellationToken)).ToSuccessResponse();
    }

    [HttpPut("reorder")]
    public async Task<ResponseResult<bool>> Reorder(
        [FromBody] ReorderEmailGroupsDto request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateCategory(request.Category);
        await groupService.ReorderAsync(
            tokenService.GetUserSqlId(),
            request.Category,
            request.EmailGroupIds,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    [HttpDelete("{emailGroupId:long}")]
    public async Task<ResponseResult<bool>> Delete(
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        await groupService.DeleteAsync(
            tokenService.GetUserSqlId(),
            emailGroupId,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    [HttpDelete("{emailGroupId:long}/invalid-recipient-contacts")]
    public async Task<ResponseResult<bool>> DeleteInvalidRecipientEmails(
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var userId = tokenService.GetUserSqlId();
        await db
            .RecipientContacts.Where(x =>
                x.EmailGroupId == emailGroupId
                && x.UserId == userId
                && x.ValidationStatus != RecipientValidationStatus.Valid
            )
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true), cancellationToken);
        return true.ToSuccessResponse();
    }

    private async Task<List<EmailGroupSummaryDto>> ToSummariesAsync(
        IReadOnlyCollection<EmailGroup> groups,
        CancellationToken cancellationToken
    )
    {
        if (groups.Count == 0)
            return [];
        var groupIds = groups.Select(x => x.Id).ToList();
        var category = groups.First().Category;
        var counts =
            category == EmailGroupCategory.EmailAccount
                ? await db
                    .EmailAccounts.Where(x => groupIds.Contains(x.EmailGroupId))
                    .GroupBy(x => x.EmailGroupId)
                    .Select(x => new { EmailGroupId = x.Key, Count = x.Count() })
                    .ToDictionaryAsync(x => x.EmailGroupId, x => x.Count, cancellationToken)
                : await db
                    .RecipientContacts.Where(x => groupIds.Contains(x.EmailGroupId))
                    .GroupBy(x => x.EmailGroupId)
                    .Select(x => new { EmailGroupId = x.Key, Count = x.Count() })
                    .ToDictionaryAsync(x => x.EmailGroupId, x => x.Count, cancellationToken);
        return groups
            .Select(group => ToSummary(group, counts.GetValueOrDefault(group.Id)))
            .ToList();
    }

    private async Task<EmailGroupSummaryDto> ToSummaryAsync(
        EmailGroup group,
        CancellationToken cancellationToken
    )
    {
        var accountCount =
            group.Category == EmailGroupCategory.EmailAccount
                ? await db.EmailAccounts.CountAsync(
                    x => x.EmailGroupId == group.Id,
                    cancellationToken
                )
                : await db.RecipientContacts.CountAsync(
                    x => x.EmailGroupId == group.Id,
                    cancellationToken
                );
        return ToSummary(group, accountCount);
    }

    private static EmailGroupSummaryDto ToSummary(EmailGroup group, int accountCount) =>
        new(
            group.Id,
            group.Category,
            group.Icon,
            group.Name,
            group.Description,
            group.Order,
            group.IsDefault,
            accountCount
        );

    private static void ValidateCategory(EmailGroupCategory category)
    {
        if (category is EmailGroupCategory.EmailAccount or EmailGroupCategory.RecipientEmail)
            return;
        throw new KnownException("不支持的邮箱分组类别");
    }
}
