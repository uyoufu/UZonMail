using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.EmailReceiving;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails;

/// <summary>
/// 邮箱账户资源。一个资源可同时配置独立保存的发送与收件能力。
/// </summary>
public sealed class EmailAccountsController(
    TokenService tokenService,
    EmailAccountManagementService accountManagementService,
    SenderAccountValidateService senderAccountValidationService,
    ReceivingAccountValidationService receivingAccountValidationService
) : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<EmailAccountDto>>> GetAll(
        [FromQuery] long? emailGroupId = null,
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default
    ) =>
        (
            await accountManagementService.GetAsync(
                tokenService.GetUserSqlId(),
                emailGroupId,
                filter,
                cancellationToken
            )
        ).ToSuccessResponse();

    /// <summary>
    /// 发送任务只需要发送能力 ID，不需要账户管理凭据或收件配置。
    /// </summary>
    [HttpGet("senders")]
    public async Task<ResponseResult<List<SenderEmailAccountOptionDto>>> GetSenderOptions(
        CancellationToken cancellationToken = default
    ) =>
        (
            await accountManagementService.GetSenderOptionsAsync(
                tokenService.GetUserSqlId(),
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpGet("{emailAccountId:long}")]
    public async Task<ResponseResult<EmailAccountDto>> Get(
        long emailAccountId,
        CancellationToken cancellationToken = default
    ) =>
        (
            await accountManagementService.GetAsync(
                tokenService.GetUserSqlId(),
                emailAccountId,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpPost("basic")]
    public Task<ResponseResult<EmailAccountDto>> CreateBasic(
        [FromBody] EmailAccountWriteDto request,
        CancellationToken cancellationToken = default
    ) => CreateAsync(EmailAccountConfigurationKind.Basic, request, cancellationToken);

    [HttpPost("microsoft-graph")]
    public Task<ResponseResult<EmailAccountDto>> CreateMicrosoftGraph(
        [FromBody] EmailAccountWriteDto request,
        CancellationToken cancellationToken = default
    ) => CreateAsync(EmailAccountConfigurationKind.MicrosoftGraph, request, cancellationToken);

    [HttpPut("{emailAccountId:long}")]
    public async Task<ResponseResult<EmailAccountDto>> Update(
        long emailAccountId,
        [FromBody] EmailAccountWriteDto request,
        CancellationToken cancellationToken = default
    ) =>
        (
            await accountManagementService.UpdateAsync(
                tokenService.GetUserSqlId(),
                emailAccountId,
                request,
                cancellationToken
            )
        ).ToSuccessResponse();

    [HttpPut("move")]
    public async Task<ResponseResult<bool>> Move(
        [FromBody] MoveEmailAccountsDto request,
        CancellationToken cancellationToken = default
    )
    {
        await accountManagementService.MoveAsync(
            tokenService.GetUserSqlId(),
            request,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    [HttpPost("{emailAccountId:long}/validate")]
    public async Task<ResponseResult<EmailAccountDto>> Validate(
        long emailAccountId,
        CancellationToken cancellationToken = default
    ) => (await ValidateOneAsync(emailAccountId, cancellationToken)).ToSuccessResponse();

    [HttpPost("validate")]
    public async Task<ResponseResult<List<EmailAccountDto>>> ValidateMany(
        [FromBody] ValidateEmailAccountsDto request,
        CancellationToken cancellationToken = default
    )
    {
        var accountIds = request.EmailAccountIds.Distinct().ToList();
        var results = new List<EmailAccountDto>(accountIds.Count);
        foreach (var emailAccountId in accountIds)
            results.Add(await ValidateOneAsync(emailAccountId, cancellationToken));
        return results.ToSuccessResponse();
    }

    [HttpDelete("groups/{emailGroupId:long}/invalid-sender-capabilities")]
    public async Task<ResponseResult<bool>> DeleteInvalidSenderCapabilities(
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        await accountManagementService.DeleteInvalidSenderCapabilitiesAsync(
            tokenService.GetUserSqlId(),
            emailGroupId,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    [HttpDelete("{emailAccountId:long}")]
    public async Task<ResponseResult<bool>> Delete(
        long emailAccountId,
        CancellationToken cancellationToken = default
    )
    {
        await accountManagementService.DeleteAsync(
            tokenService.GetUserSqlId(),
            emailAccountId,
            cancellationToken
        );
        return true.ToSuccessResponse();
    }

    private async Task<ResponseResult<EmailAccountDto>> CreateAsync(
        EmailAccountConfigurationKind configurationKind,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken
    )
    {
        request.ConfigurationKind = configurationKind;
        var emailAccount = await accountManagementService.CreateAsync(
            tokenService.GetUserSqlId(),
            tokenService.GetOrganizationId(),
            request,
            cancellationToken
        );
        return emailAccount.ToSuccessResponse();
    }

    private async Task<EmailAccountDto> ValidateOneAsync(
        long emailAccountId,
        CancellationToken cancellationToken
    )
    {
        var userId = tokenService.GetUserSqlId();
        var account = await accountManagementService.GetAsync(
            userId,
            emailAccountId,
            cancellationToken
        );
        if (account.Sender != null)
            await senderAccountValidationService.ValidateSenderAccount(account.Sender.Id);
        if (account.Receiving != null)
        {
            try
            {
                await receivingAccountValidationService.ValidateAsync(
                    userId,
                    account.Receiving.Id,
                    cancellationToken
                );
            }
            catch (KnownException)
            {
                // 收件验证服务已落库错误状态；统一列表必须返回最新验证结果。
            }
        }

        return await accountManagementService.GetAsync(userId, emailAccountId, cancellationToken);
    }
}
