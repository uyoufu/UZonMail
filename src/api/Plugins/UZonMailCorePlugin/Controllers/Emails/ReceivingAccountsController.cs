using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.CorePlugin.Services.EmailReceiving;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Emails;

/// <summary>
/// 收件能力的配置、关联与认证验证接口。
/// </summary>
public sealed class ReceivingAccountsController(
    SqlContext db,
    TokenService tokenService,
    EmailAccountService emailAccountService,
    AccountCredentialService credentialService,
    ReceivingAccountValidationService validationService
) : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<ReceivingAccountDto>>> GetAll()
    {
        return (await QueryDtos(tokenService.GetUserSqlId()).ToListAsync()).ToSuccessResponse();
    }

    [HttpGet("{receivingAccountId:long}")]
    public async Task<ResponseResult<ReceivingAccountDto>> Get(long receivingAccountId)
    {
        var dto =
            await QueryDtos(tokenService.GetUserSqlId())
                .FirstOrDefaultAsync(x => x.Id == receivingAccountId)
            ?? throw new KnownException("收件账户不存在");
        return dto.ToSuccessResponse();
    }

    [HttpPost("basic")]
    public async Task<ResponseResult<ReceivingAccountDto>> CreateBasic(
        [FromBody] CreateBasicReceivingAccountDto request
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var account = await CreateAccountAsync(
            request.Email,
            request.Name,
            request.Description,
            request.Remark,
            request.ContentRetentionDays,
            ReceivingProtocol.Imap,
            AuthenticationMethod.Password
        );
        await credentialService.SetImapCredentialAsync(
            account,
            new ProtocolCredentialInput(
                request.Credential.Host,
                request.Credential.Port,
                request.Credential.ConnectionSecurity,
                request.Credential.LoginName,
                request.Credential.Password
            )
        );
        await ConfigureSenderLinksAsync(
            account,
            request.SenderAccountIds,
            request.PrimarySenderAccountId
        );
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCreatedDto(account.Id);
    }

    [HttpPost("microsoft-graph")]
    public async Task<ResponseResult<ReceivingAccountDto>> CreateMicrosoftGraph(
        [FromBody] CreateMicrosoftGraphReceivingAccountDto request
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var account = await CreateAccountAsync(
            request.Email,
            request.Name,
            request.Description,
            request.Remark,
            request.ContentRetentionDays,
            ReceivingProtocol.MicrosoftGraph,
            AuthenticationMethod.OAuth2
        );
        await credentialService.ConfigureMicrosoftOAuthAsync(
            account.EmailAccount,
            new MicrosoftOAuthApplicationInput(
                request.Application.ApplicationSource,
                request.Application.TenantId,
                request.Application.ClientId,
                request.Application.ClientSecret
            )
        );
        await ConfigureSenderLinksAsync(
            account,
            request.SenderAccountIds,
            request.PrimarySenderAccountId
        );
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCreatedDto(account.Id);
    }

    [HttpPost("oauth")]
    public ResponseResult<bool> CreateGenericOAuthReserved() =>
        false.ToFailResponse("通用 IMAP OAuth2 仅保留模型，本版本暂不开放");

    [HttpPut("{receivingAccountId:long}")]
    public async Task<ResponseResult<ReceivingAccountDto>> Update(
        long receivingAccountId,
        [FromBody] UpdateReceivingAccountDto request
    )
    {
        var account = await FindOwnedAccountAsync(receivingAccountId);
        account.EmailAccount.Name = request.Name;
        account.EmailAccount.Description = request.Description;
        account.EmailAccount.Remark = request.Remark;
        account.ContentRetentionDays = ValidateRetentionDays(request.ContentRetentionDays);
        await ConfigureSenderLinksAsync(
            account,
            request.SenderAccountIds,
            request.PrimarySenderAccountId
        );
        await db.SaveChangesAsync();
        return await GetCreatedDto(account.Id);
    }

    [HttpPut("{receivingAccountId:long}/imap-credential")]
    public async Task<ResponseResult<ReceivingAccountDto>> UpdateImapCredential(
        long receivingAccountId,
        [FromBody] ImapCredentialWriteDto request
    )
    {
        var account = await FindOwnedAccountAsync(receivingAccountId);
        if (
            account.Protocol != ReceivingProtocol.Imap
            || account.AuthenticationMethod != AuthenticationMethod.Password
        )
            throw new KnownException("该收件账户不是 IMAP Password 账户");

        await credentialService.SetImapCredentialAsync(
            account,
            new ProtocolCredentialInput(
                request.Host,
                request.Port,
                request.ConnectionSecurity,
                request.LoginName,
                request.Password
            )
        );
        await db.SaveChangesAsync();
        return await GetCreatedDto(account.Id);
    }

    [HttpPut("{receivingAccountId:long}/microsoft-graph-application")]
    public async Task<ResponseResult<ReceivingAccountDto>> UpdateMicrosoftGraphApplication(
        long receivingAccountId,
        [FromBody] MicrosoftGraphApplicationWriteDto request
    )
    {
        var account = await FindOwnedAccountAsync(receivingAccountId);
        if (account.Protocol != ReceivingProtocol.MicrosoftGraph)
            throw new KnownException("该收件账户不是 Microsoft Graph 账户");
        await credentialService.ConfigureMicrosoftOAuthAsync(
            account.EmailAccount,
            new MicrosoftOAuthApplicationInput(
                request.ApplicationSource,
                request.TenantId,
                request.ClientId,
                request.ClientSecret
            )
        );
        account.Status = ReceivingAccountStatus.Unverified;
        if (account.EmailAccount.SenderAccount != null)
            account.EmailAccount.SenderAccount.Status = SenderAccountStatus.Unverified;
        await db.SaveChangesAsync();
        return await GetCreatedDto(account.Id);
    }

    [HttpPost("{receivingAccountId:long}/validate")]
    public async Task<ResponseResult<bool>> Validate(long receivingAccountId)
    {
        await validationService.ValidateAsync(tokenService.GetUserSqlId(), receivingAccountId);
        return true.ToSuccessResponse();
    }

    [HttpDelete("{receivingAccountId:long}")]
    public async Task<ResponseResult<bool>> Delete(long receivingAccountId)
    {
        var account = await FindOwnedAccountAsync(receivingAccountId);
        await db
            .ReceivingAccountPrimarySenders.Where(x => x.ReceivingAccountId == account.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        await db
            .ReceivingAccountSenderLinks.Where(x => x.ReceivingAccountId == account.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        await db
            .ReceivingAccountImapCredentials.Where(x => x.ReceivingAccountId == account.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        account.IsDeleted = true;
        if (!await db.SenderAccounts.AnyAsync(x => x.EmailAccountId == account.EmailAccountId))
        {
            account.EmailAccount.IsDeleted = true;
            await db
                .EmailAccountOAuthCredentials.Where(x => x.EmailAccountId == account.EmailAccountId)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        }
        await db.SaveChangesAsync();
        return true.ToSuccessResponse();
    }

    private async Task<ReceivingAccount> CreateAccountAsync(
        string email,
        string? name,
        string? description,
        string? remark,
        int retentionDays,
        ReceivingProtocol protocol,
        AuthenticationMethod authenticationMethod
    )
    {
        var userId = tokenService.GetUserSqlId();
        var emailAccount = await emailAccountService.GetOrCreateAsync(
            userId,
            tokenService.GetOrganizationId(),
            email,
            name,
            description,
            remark
        );
        await db.SaveChangesAsync();
        if (await db.ReceivingAccounts.AnyAsync(x => x.EmailAccountId == emailAccount.Id))
            throw new KnownException("该邮箱已经具有收件能力");

        var account = new ReceivingAccount
        {
            EmailAccount = emailAccount,
            Protocol = protocol,
            AuthenticationMethod = authenticationMethod,
            Status = ReceivingAccountStatus.Unverified,
            ContentRetentionDays = ValidateRetentionDays(retentionDays),
        };
        db.ReceivingAccounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    private async Task<ReceivingAccount> FindOwnedAccountAsync(long receivingAccountId)
    {
        var userId = tokenService.GetUserSqlId();
        return await db
                .ReceivingAccounts.Include(x => x.EmailAccount)
                .ThenInclude(x => x.SenderAccount)
                .FirstOrDefaultAsync(x =>
                    x.Id == receivingAccountId && x.EmailAccount.UserId == userId
                ) ?? throw new KnownException("收件账户不存在");
    }

    private async Task ConfigureSenderLinksAsync(
        ReceivingAccount receivingAccount,
        IReadOnlyCollection<long> senderAccountIds,
        long? primarySenderAccountId
    )
    {
        var distinctIds = senderAccountIds.Distinct().ToList();
        if (primarySenderAccountId.HasValue && !distinctIds.Contains(primarySenderAccountId.Value))
            throw new KnownException("主要发件账户必须包含在关联发件账户中");

        var userId = tokenService.GetUserSqlId();
        var ownedIds = await db
            .SenderAccounts.Where(x =>
                x.EmailAccount.UserId == userId && distinctIds.Contains(x.Id)
            )
            .Select(x => x.Id)
            .ToListAsync();
        if (ownedIds.Count != distinctIds.Count)
            throw new KnownException("关联发件账户不存在或不属于当前用户");

        var existingLinks = await db
            .ReceivingAccountSenderLinks.IgnoreQueryFilters()
            .Where(x => x.ReceivingAccountId == receivingAccount.Id)
            .ToListAsync();
        foreach (var link in existingLinks)
            link.IsDeleted = !distinctIds.Contains(link.SenderAccountId);
        foreach (
            var senderAccountId in distinctIds.Except(existingLinks.Select(x => x.SenderAccountId))
        )
        {
            existingLinks.Add(
                new ReceivingAccountSenderLink
                {
                    ReceivingAccountId = receivingAccount.Id,
                    SenderAccountId = senderAccountId,
                }
            );
            db.ReceivingAccountSenderLinks.Add(existingLinks[^1]);
        }
        await db.SaveChangesAsync();

        var primary = await db
            .ReceivingAccountPrimarySenders.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ReceivingAccountId == receivingAccount.Id);
        if (!primarySenderAccountId.HasValue)
        {
            if (primary != null)
                primary.IsDeleted = true;
            return;
        }

        var primaryLink = existingLinks.First(x =>
            x.SenderAccountId == primarySenderAccountId.Value && !x.IsDeleted
        );
        if (primary == null)
        {
            db.ReceivingAccountPrimarySenders.Add(
                new ReceivingAccountPrimarySender
                {
                    ReceivingAccountId = receivingAccount.Id,
                    ReceivingAccountSenderLinkId = primaryLink.Id,
                }
            );
            return;
        }
        primary.IsDeleted = false;
        primary.ReceivingAccountSenderLinkId = primaryLink.Id;
    }

    private IQueryable<ReceivingAccountDto> QueryDtos(long userId) =>
        db
            .ReceivingAccounts.Where(x => x.EmailAccount.UserId == userId)
            .Select(x => new ReceivingAccountDto(
                x.Id,
                x.EmailAccountId,
                x.EmailAccount.Email,
                x.EmailAccount.Name,
                x.Protocol,
                x.AuthenticationMethod,
                x.Status,
                x.ContentRetentionDays,
                x.LastConnectedAtUtc,
                x.LastError,
                db.ReceivingAccountImapCredentials.Any(c => c.ReceivingAccountId == x.Id),
                db.EmailAccountOAuthCredentials.Where(c => c.EmailAccountId == x.EmailAccountId)
                    .Select(c => (OAuthApplicationSource?)c.ApplicationSource)
                    .FirstOrDefault(),
                db.EmailAccountOAuthCredentials.Any(c =>
                    c.EmailAccountId == x.EmailAccountId
                    && c.EncryptedRefreshToken != null
                    && c.EncryptedRefreshToken != ""
                ),
                db.ReceivingAccountSenderLinks.Where(link => link.ReceivingAccountId == x.Id)
                    .Select(link => link.SenderAccountId)
                    .ToList(),
                db.ReceivingAccountPrimarySenders.Where(primary =>
                    primary.ReceivingAccountId == x.Id
                )
                    .Select(primary => (long?)primary.ReceivingAccountSenderLink.SenderAccountId)
                    .FirstOrDefault()
            ));

    private async Task<ResponseResult<ReceivingAccountDto>> GetCreatedDto(long receivingAccountId)
    {
        var dto = await QueryDtos(tokenService.GetUserSqlId())
            .FirstAsync(x => x.Id == receivingAccountId);
        return dto.ToSuccessResponse();
    }

    private static int ValidateRetentionDays(int retentionDays)
    {
        if (retentionDays is < 1 or > 3650)
            throw new KnownException("邮件保留天数必须在 1 到 3650 之间");
        return retentionDays;
    }
}
