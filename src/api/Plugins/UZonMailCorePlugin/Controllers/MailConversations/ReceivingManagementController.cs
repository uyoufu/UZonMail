using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.CorePlugin.Services.EmailReceiving;
using UzonMail.CorePlugin.Services.MailConversations;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.MailConversations;

/// <summary>
/// 收件账号状态和手动同步入口。
/// </summary>
public sealed class ReceivingManagementController(
    TokenService tokenService,
    ReceivingAccountQueryService accountQueryService,
    IReceivingSynchronizationService synchronizationService
) : ControllerBaseV1
{
    [HttpGet("accounts")]
    public async Task<ResponseResult<List<ReceivingAccountSummaryDto>>> GetAccounts(
        CancellationToken cancellationToken
    ) =>
        (
            await accountQueryService.GetAsync(tokenService.GetUserSqlId(), cancellationToken)
        ).ToSuccessResponse();

    [HttpPost("accounts/{receivingAccountId:long}/sync")]
    public async Task<ResponseResult<ReceivingSynchronizationResult>> Synchronize(
        long receivingAccountId,
        CancellationToken cancellationToken
    ) =>
        (
            await synchronizationService.SynchronizeAsync(
                tokenService.GetUserSqlId(),
                receivingAccountId,
                ImapSyncTrigger.Manual,
                cancellationToken
            )
        ).ToSuccessResponse();
}
