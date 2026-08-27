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
        var query = QueryDtos(userId);
        if (emailGroupId.HasValue)
            query = query.Where(x => x.EmailGroupId == emailGroupId.Value);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var normalizedFilter = filter.Trim().ToLower();
            query = query.Where(x =>
                x.Email.ToLower().Contains(normalizedFilter)
                || (x.Name != null && x.Name.ToLower().Contains(normalizedFilter))
            );
        }
        return (await query.ToListAsync()).ToSuccessResponse();
    }

    [HttpGet("{recipientContactId:long}")]
    public async Task<ResponseResult<RecipientContactDto>> Get(long recipientContactId)
    {
        var dto =
            await QueryDtos(tokenService.GetUserSqlId())
                .FirstOrDefaultAsync(x => x.Id == recipientContactId)
            ?? throw new KnownException("收件人不存在");
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
            await QueryDtos(entity.UserId).FirstAsync(x => x.Id == entity.Id)
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
            await QueryDtos(tokenService.GetUserSqlId())
                .Where(x => createdIds.Contains(x.Id))
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
        return (await QueryDtos(userId).FirstAsync(x => x.Id == contact.Id)).ToSuccessResponse();
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
                && x.Category == EmailGroupCategory.Recipient
            )
        )
            throw new KnownException("收件人分组不存在");
    }

    private IQueryable<RecipientContactDto> QueryDtos(long userId) =>
        db
            .RecipientContacts.Where(x => x.UserId == userId)
            .Select(x => new RecipientContactDto(
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
