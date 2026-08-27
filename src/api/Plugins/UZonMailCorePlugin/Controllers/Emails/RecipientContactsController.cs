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
/// 邮件投递目标联系人资源接口。
/// </summary>
public sealed class RecipientContactsController(SqlContext db, TokenService tokenService)
    : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<RecipientContactDto>>> GetAll(
        [FromQuery] long? emailGroupId = null,
        [FromQuery] string? filter = null
    )
    {
        var userId = tokenService.GetUserSqlId();
        var recipientContacts = QueryRecipientContacts(userId);
        if (emailGroupId.HasValue)
            recipientContacts = recipientContacts.Where(x => x.EmailGroupId == emailGroupId.Value);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var normalizedFilter = filter.Trim().ToLower();
            recipientContacts = recipientContacts.Where(x =>
                x.Email.ToLower().Contains(normalizedFilter)
                || (x.Name != null && x.Name.ToLower().Contains(normalizedFilter))
            );
        }
        return (await SelectDtos(recipientContacts).ToListAsync()).ToSuccessResponse();
    }

    [HttpGet("{recipientContactId:long}")]
    public async Task<ResponseResult<RecipientContactDto>> Get(long recipientContactId)
    {
        var dto =
            await SelectDtos(
                    QueryRecipientContacts(tokenService.GetUserSqlId())
                        .Where(x => x.Id == recipientContactId)
                )
                .FirstOrDefaultAsync() ?? throw new KnownException("收件人不存在");
        return dto.ToSuccessResponse();
    }

    [HttpPost]
    public async Task<ResponseResult<RecipientContactDto>> Create(
        [FromBody] CreateRecipientContactDto request
    )
    {
        var entity = await CreateEntityAsync(request);
        await db.SaveChangesAsync();
        return (
            await SelectDtos(QueryRecipientContacts(entity.UserId).Where(x => x.Id == entity.Id))
                .FirstAsync()
        ).ToSuccessResponse();
    }

    [HttpPost("batch")]
    public async Task<ResponseResult<List<RecipientContactDto>>> CreateBatch(
        [FromBody] List<CreateRecipientContactDto> requests
    )
    {
        if (requests.Count == 0)
            return new List<RecipientContactDto>().ToSuccessResponse();
        if (requests.Count > 1000)
            throw new KnownException("单次最多创建 1000 个收件人");

        var duplicateEmails = requests
            .GroupBy(
                request => EmailAccountService.NormalizeEmail(request.Email),
                StringComparer.Ordinal
            )
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateEmails.Count > 0)
            throw new KnownException($"批量请求包含重复收件人：{string.Join(", ", duplicateEmails)}");

        var created = new List<RecipientContact>();
        foreach (var request in requests)
            created.Add(await CreateEntityAsync(request));
        await db.SaveChangesAsync();
        var createdIds = created.Select(x => x.Id).ToList();
        return (
            await SelectDtos(
                    QueryRecipientContacts(tokenService.GetUserSqlId())
                        .Where(x => createdIds.Contains(x.Id))
                )
                .ToListAsync()
        ).ToSuccessResponse();
    }

    [HttpPut("{recipientContactId:long}")]
    public async Task<ResponseResult<RecipientContactDto>> Update(
        long recipientContactId,
        [FromBody] UpdateRecipientContactDto request
    )
    {
        var userId = tokenService.GetUserSqlId();
        await EnsureRecipientGroupAsync(userId, request.EmailGroupId);
        var contact =
            await db.RecipientContacts.FirstOrDefaultAsync(x =>
                x.Id == recipientContactId && x.UserId == userId
            ) ?? throw new KnownException("收件人不存在");
        var normalizedEmail = EmailAccountService.NormalizeEmail(request.Email);
        if (
            await db.RecipientContacts.AnyAsync(x =>
                x.UserId == userId
                && x.NormalizedEmail == normalizedEmail
                && x.Id != recipientContactId
            )
        )
            throw new KnownException("收件人邮箱已存在");

        contact.EmailGroupId = request.EmailGroupId;
        contact.Email = request.Email;
        contact.Name = request.Name;
        contact.Description = request.Description;
        contact.Remark = request.Remark;
        contact.MinimumCooldownHours = request.MinimumCooldownHours;
        await db.SaveChangesAsync();
        return (
            await SelectDtos(QueryRecipientContacts(userId).Where(x => x.Id == contact.Id))
                .FirstAsync()
        ).ToSuccessResponse();
    }

    [HttpPut("validation-status")]
    public async Task<ResponseResult<bool>> UpdateValidationStatus(
        [FromBody] UpdateRecipientValidationStatusDto request
    )
    {
        var userId = tokenService.GetUserSqlId();
        await db
            .RecipientContacts.Where(x =>
                x.UserId == userId && request.RecipientContactIds.Contains(x.Id)
            )
            .ExecuteUpdateAsync(x =>
                x.SetProperty(y => y.ValidationStatus, request.ValidationStatus)
                    .SetProperty(y => y.ValidationFailureReason, (string?)null)
            );
        return true.ToSuccessResponse();
    }

    [HttpDelete("{recipientContactId:long}")]
    public async Task<ResponseResult<bool>> Delete(long recipientContactId)
    {
        var userId = tokenService.GetUserSqlId();
        var updated = await db
            .RecipientContacts.Where(x => x.Id == recipientContactId && x.UserId == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        if (updated == 0)
            throw new KnownException("收件人不存在");
        return true.ToSuccessResponse();
    }

    private async Task<RecipientContact> CreateEntityAsync(CreateRecipientContactDto request)
    {
        var userId = tokenService.GetUserSqlId();
        await EnsureRecipientGroupAsync(userId, request.EmailGroupId);
        var normalizedEmail = EmailAccountService.NormalizeEmail(request.Email);
        if (
            await db.RecipientContacts.AnyAsync(x =>
                x.UserId == userId && x.NormalizedEmail == normalizedEmail
            )
        )
            throw new KnownException($"收件人 {request.Email} 已存在");

        var contact = new RecipientContact
        {
            UserId = userId,
            OrganizationId = tokenService.GetOrganizationId(),
            EmailGroupId = request.EmailGroupId,
            Email = request.Email,
            Name = request.Name,
            Description = request.Description,
            Remark = request.Remark,
            MinimumCooldownHours = request.MinimumCooldownHours,
            ValidationStatus = RecipientValidationStatus.Unverified,
        };
        db.RecipientContacts.Add(contact);
        return contact;
    }

    private async Task EnsureRecipientGroupAsync(long userId, long emailGroupId)
    {
        if (
            !await db.EmailGroups.AnyAsync(x =>
                x.Id == emailGroupId
                && x.UserId == userId
                && x.Category == EmailGroupCategory.RecipientEmail
            )
        )
            throw new KnownException("收件人分组不存在");
    }

    private IQueryable<RecipientContact> QueryRecipientContacts(long userId) =>
        db.RecipientContacts.Where(x => x.UserId == userId);

    /// <summary>
    /// 保持所有 SQL 筛选作用于实体字段，避免 EF Core 继续翻译 record DTO 的成员访问。
    /// </summary>
    private static IQueryable<RecipientContactDto> SelectDtos(
        IQueryable<RecipientContact> recipientContacts
    ) =>
        recipientContacts.Select(x => new RecipientContactDto(
            x.Id,
            x.EmailGroupId,
            x.Email,
            x.Name,
            x.Description,
            x.Remark,
            x.MinimumCooldownHours,
            x.ValidationStatus,
            x.ValidationFailureReason,
            x.LastDeliveredAtUtc,
            x.LastSuccessDeliveryDate
        ));
}

public sealed class UpdateRecipientValidationStatusDto
{
    public List<long> RecipientContactIds { get; set; } = [];
    public RecipientValidationStatus ValidationStatus { get; set; }
}
