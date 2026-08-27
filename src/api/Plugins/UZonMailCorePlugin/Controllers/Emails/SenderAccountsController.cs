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
/// 发件能力资源接口，普通查询不加载协议密钥。
/// </summary>
public sealed class SenderAccountsController(
    SqlContext db,
    TokenService tokenService,
    EmailAccountService emailAccountService,
    AccountCredentialService credentialService,
    SenderAccountValidateService validationService
) : ControllerBaseV1
{
    [HttpGet]
    public async Task<ResponseResult<List<SenderAccountDto>>> GetAll(
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

    [HttpGet("{senderAccountId:long}")]
    public async Task<ResponseResult<SenderAccountDto>> Get(long senderAccountId)
    {
        var userId = tokenService.GetUserSqlId();
        var dto =
            await QueryDtos(userId).FirstOrDefaultAsync(x => x.Id == senderAccountId)
            ?? throw new KnownException("发件账户不存在");
        return dto.ToSuccessResponse();
    }

    [HttpPost("smtp")]
    public async Task<ResponseResult<SenderAccountDto>> CreateSmtp(
        [FromBody] CreateSmtpSenderAccountDto request
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var senderAccount = await CreateAccountAsync(
            request.Email,
            request.Name,
            request.Description,
            request.Remark,
            request.EmailGroupId,
            SendingProtocol.Smtp,
            AuthenticationMethod.Password,
            request.ProxyId,
            request.MaxSendCountPerDay,
            request.ReplyToEmails,
            request.Weight
        );
        await credentialService.SetSmtpCredentialAsync(
            senderAccount,
            ToCredentialInput(request.Credential)
        );
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCreatedDto(senderAccount.Id);
    }

    [HttpPost("microsoft-graph")]
    public async Task<ResponseResult<SenderAccountDto>> CreateMicrosoftGraph(
        [FromBody] CreateMicrosoftGraphSenderAccountDto request
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var senderAccount = await CreateAccountAsync(
            request.Email,
            request.Name,
            request.Description,
            request.Remark,
            request.EmailGroupId,
            SendingProtocol.MicrosoftGraph,
            AuthenticationMethod.OAuth2,
            null,
            request.MaxSendCountPerDay,
            request.ReplyToEmails,
            request.Weight
        );
        await credentialService.ConfigureMicrosoftOAuthAsync(
            senderAccount.EmailAccount,
            ToOAuthInput(request.Application)
        );
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetCreatedDto(senderAccount.Id);
    }

    [HttpPut("{senderAccountId:long}")]
    public async Task<ResponseResult<SenderAccountDto>> Update(
        long senderAccountId,
        [FromBody] UpdateSenderAccountDto request
    )
    {
        var userId = tokenService.GetUserSqlId();
        await EnsureSenderGroupAsync(userId, request.EmailGroupId);
        var senderAccount =
            await db
                .SenderAccounts.Include(x => x.EmailAccount)
                .FirstOrDefaultAsync(x =>
                    x.Id == senderAccountId && x.EmailAccount.UserId == userId
                ) ?? throw new KnownException("发件账户不存在");

        senderAccount.EmailAccount.Name = request.Name;
        senderAccount.EmailAccount.Description = request.Description;
        senderAccount.EmailAccount.Remark = request.Remark;
        senderAccount.EmailGroupId = request.EmailGroupId;
        senderAccount.ProxyId = request.ProxyId;
        senderAccount.MaxSendCountPerDay = Math.Max(0, request.MaxSendCountPerDay);
        senderAccount.ReplyToEmails = request.ReplyToEmails;
        senderAccount.Weight = Math.Max(1, request.Weight);
        await db.SaveChangesAsync();
        return await GetCreatedDto(senderAccount.Id);
    }

    [HttpPut("{senderAccountId:long}/smtp-credential")]
    public async Task<ResponseResult<SenderAccountDto>> UpdateSmtpCredential(
        long senderAccountId,
        [FromBody] SmtpCredentialWriteDto request
    )
    {
        var senderAccount = await FindOwnedSenderAsync(senderAccountId);
        if (
            senderAccount.Protocol != SendingProtocol.Smtp
            || senderAccount.AuthenticationMethod != AuthenticationMethod.Password
        )
            throw new KnownException("该发件账户不是 SMTP Password 账户");

        await credentialService.SetSmtpCredentialAsync(senderAccount, ToCredentialInput(request));
        await db.SaveChangesAsync();
        return await GetCreatedDto(senderAccount.Id);
    }

    [HttpPut("{senderAccountId:long}/microsoft-graph-application")]
    public async Task<ResponseResult<SenderAccountDto>> UpdateMicrosoftGraphApplication(
        long senderAccountId,
        [FromBody] MicrosoftGraphApplicationWriteDto request
    )
    {
        var senderAccount = await FindOwnedSenderAsync(senderAccountId);
        if (senderAccount.Protocol != SendingProtocol.MicrosoftGraph)
            throw new KnownException("该发件账户不是 Microsoft Graph 账户");

        await credentialService.ConfigureMicrosoftOAuthAsync(
            senderAccount.EmailAccount,
            ToOAuthInput(request)
        );
        senderAccount.Status = SenderAccountStatus.Unverified;
        senderAccount.ValidationFailureReason = null;
        await db.SaveChangesAsync();
        return await GetCreatedDto(senderAccount.Id);
    }

    [HttpPost("{senderAccountId:long}/validate")]
    public Task<ResponseResult<bool>> Validate(long senderAccountId) =>
        validationService.ValidateSenderAccount(senderAccountId);

    [HttpDelete("{senderAccountId:long}")]
    public async Task<ResponseResult<bool>> Delete(long senderAccountId)
    {
        var senderAccount = await FindOwnedSenderAsync(senderAccountId);
        var primaryLinks = db.ReceivingAccountPrimarySenders.Where(x =>
            x.ReceivingAccountSenderLink.SenderAccountId == senderAccountId
        );
        await primaryLinks.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        await db
            .ReceivingAccountSenderLinks.Where(x => x.SenderAccountId == senderAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        await db
            .SenderAccountSmtpCredentials.Where(x => x.SenderAccountId == senderAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        senderAccount.IsDeleted = true;
        if (
            !await db.ReceivingAccounts.AnyAsync(x =>
                x.EmailAccountId == senderAccount.EmailAccountId
            )
        )
            senderAccount.EmailAccount.IsDeleted = true;
        await db.SaveChangesAsync();
        return true.ToSuccessResponse();
    }

    private async Task<SenderAccount> CreateAccountAsync(
        string email,
        string? name,
        string? description,
        string? remark,
        long emailGroupId,
        SendingProtocol protocol,
        AuthenticationMethod authenticationMethod,
        long? proxyId,
        int maxSendCountPerDay,
        string? replyToEmails,
        int weight
    )
    {
        var userId = tokenService.GetUserSqlId();
        await EnsureSenderGroupAsync(userId, emailGroupId);
        var emailAccount = await emailAccountService.GetOrCreateAsync(
            userId,
            tokenService.GetOrganizationId(),
            email,
            name,
            description,
            remark
        );
        await db.SaveChangesAsync();
        if (await db.SenderAccounts.AnyAsync(x => x.EmailAccountId == emailAccount.Id))
            throw new KnownException("该邮箱已经具有发件能力");

        var senderAccount = new SenderAccount
        {
            EmailAccount = emailAccount,
            EmailGroupId = emailGroupId,
            Protocol = protocol,
            AuthenticationMethod = authenticationMethod,
            ProxyId = proxyId,
            MaxSendCountPerDay = Math.Max(0, maxSendCountPerDay),
            ReplyToEmails = replyToEmails,
            Weight = Math.Max(1, weight),
            Status = SenderAccountStatus.Unverified,
        };
        db.SenderAccounts.Add(senderAccount);
        await db.SaveChangesAsync();
        return senderAccount;
    }

    private async Task<SenderAccount> FindOwnedSenderAsync(long senderAccountId)
    {
        var userId = tokenService.GetUserSqlId();
        return await db
                .SenderAccounts.Include(x => x.EmailAccount)
                .FirstOrDefaultAsync(x =>
                    x.Id == senderAccountId && x.EmailAccount.UserId == userId
                ) ?? throw new KnownException("发件账户不存在");
    }

    private async Task EnsureSenderGroupAsync(long userId, long emailGroupId)
    {
        if (
            !await db.EmailGroups.AnyAsync(x =>
                x.Id == emailGroupId
                && x.UserId == userId
                && x.Category == EmailGroupCategory.Sender
            )
        )
            throw new KnownException("发件账户分组不存在");
    }

    private IQueryable<SenderAccountDto> QueryDtos(long userId) =>
        db
            .SenderAccounts.Where(x => x.EmailAccount.UserId == userId)
            .Select(x => new SenderAccountDto(
                x.Id,
                x.EmailAccountId,
                x.EmailGroupId,
                x.EmailAccount.Email,
                x.EmailAccount.Name,
                x.EmailAccount.Description,
                x.EmailAccount.Remark,
                x.Protocol,
                x.AuthenticationMethod,
                x.Status,
                x.ValidationFailureReason,
                x.ProxyId,
                x.MaxSendCountPerDay,
                x.SentTotalToday,
                x.ReplyToEmails,
                x.Weight,
                db.SenderAccountSmtpCredentials.Any(c => c.SenderAccountId == x.Id),
                db.EmailAccountOAuthCredentials.Where(c => c.EmailAccountId == x.EmailAccountId)
                    .Select(c => (OAuthApplicationSource?)c.ApplicationSource)
                    .FirstOrDefault(),
                db.EmailAccountOAuthCredentials.Any(c =>
                    c.EmailAccountId == x.EmailAccountId
                    && c.EncryptedRefreshToken != null
                    && c.EncryptedRefreshToken != ""
                )
            ));

    private async Task<ResponseResult<SenderAccountDto>> GetCreatedDto(long senderAccountId)
    {
        var dto = await QueryDtos(tokenService.GetUserSqlId())
            .FirstAsync(x => x.Id == senderAccountId);
        return dto.ToSuccessResponse();
    }

    private static ProtocolCredentialInput ToCredentialInput(SmtpCredentialWriteDto request) =>
        new(
            request.Host,
            request.Port,
            request.ConnectionSecurity,
            request.LoginName,
            request.Password
        );

    private static MicrosoftOAuthApplicationInput ToOAuthInput(
        MicrosoftGraphApplicationWriteDto request
    ) => new(request.ApplicationSource, request.TenantId, request.ClientId, request.ClientSecret);
}
