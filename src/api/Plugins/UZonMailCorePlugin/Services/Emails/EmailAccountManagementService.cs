using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails;

/// <summary>
/// 邮箱身份及其独立发送、收件能力的唯一写入边界。
/// 凭据写入委托给 <see cref="AccountCredentialService"/>，防止敏感字段流入读取模型。
/// </summary>
public sealed class EmailAccountManagementService(
    SqlContext db,
    EmailAccountService emailAccountService,
    AccountCredentialService credentialService
) : IScopedService
{
    public async Task<List<EmailAccountDto>> GetAsync(
        long userId,
        long? emailGroupId,
        string? filter,
        CancellationToken cancellationToken = default
    )
    {
        var emailAccounts = QueryEmailAccounts(userId);
        if (emailGroupId.HasValue)
            emailAccounts = emailAccounts.Where(x => x.EmailGroupId == emailGroupId.Value);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var normalizedFilter = filter.Trim().ToLowerInvariant();
            emailAccounts = emailAccounts.Where(x =>
                x.Email.ToLower().Contains(normalizedFilter)
                || (x.Name != null && x.Name.ToLower().Contains(normalizedFilter))
            );
        }

        return await SelectDtos(emailAccounts.OrderBy(x => x.Email)).ToListAsync(cancellationToken);
    }

    public async Task<EmailAccountDto> GetAsync(
        long userId,
        long emailAccountId,
        CancellationToken cancellationToken = default
    ) =>
        await SelectDtos(QueryEmailAccounts(userId).Where(x => x.Id == emailAccountId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new KnownException("邮箱账户不存在");

    public async Task<List<SenderEmailAccountOptionDto>> GetSenderOptionsAsync(
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        await db
            .SenderAccounts.AsNoTracking()
            .Where(x => x.EmailAccount.UserId == userId)
            .OrderBy(x => x.EmailAccount.Email)
            .Select(x => new SenderEmailAccountOptionDto(
                x.Id,
                x.EmailAccount.Email,
                x.EmailAccount.Name,
                x.EmailAccount.Description
            ))
            .ToListAsync(cancellationToken);

    public async Task<EmailAccountDto> CreateAsync(
        long userId,
        long organizationId,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateConfiguration(request);
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new KnownException("邮箱地址不能为空");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await EnsureEmailAccountGroupAsync(userId, request.EmailGroupId, cancellationToken);
        var emailAccount = await emailAccountService.GetOrCreateAsync(
            userId,
            organizationId,
            request.Email,
            request.Name,
            request.Description,
            request.Remark,
            cancellationToken
        );
        emailAccount.EmailGroupId = request.EmailGroupId;
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(emailAccount).Reference(x => x.OAuthCredential).LoadAsync(cancellationToken);

        await ApplyCapabilitiesAsync(emailAccount, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(userId, emailAccount.Id, cancellationToken);
    }

    public async Task<EmailAccountDto> UpdateAsync(
        long userId,
        long emailAccountId,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateConfiguration(request);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await EnsureEmailAccountGroupAsync(userId, request.EmailGroupId, cancellationToken);
        var emailAccount = await FindOwnedAccountAsync(userId, emailAccountId, cancellationToken);
        emailAccount.EmailGroupId = request.EmailGroupId;
        emailAccount.Name = request.Name;
        emailAccount.Description = request.Description;
        emailAccount.Remark = request.Remark;
        await db.SaveChangesAsync(cancellationToken);

        await ApplyCapabilitiesAsync(emailAccount, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(userId, emailAccount.Id, cancellationToken);
    }

    public async Task MoveAsync(
        long userId,
        MoveEmailAccountsDto request,
        CancellationToken cancellationToken = default
    )
    {
        var accountIds = request.EmailAccountIds.Distinct().ToList();
        if (accountIds.Count == 0)
            return;

        await EnsureEmailAccountGroupAsync(userId, request.TargetEmailGroupId, cancellationToken);
        var matchedCount = await db
            .EmailAccounts.Where(x => x.UserId == userId && accountIds.Contains(x.Id))
            .ExecuteUpdateAsync(
                setter => setter.SetProperty(x => x.EmailGroupId, request.TargetEmailGroupId),
                cancellationToken
            );
        if (matchedCount != accountIds.Count)
            throw new KnownException("部分邮箱账户不存在或不属于当前用户");
    }

    public async Task DeleteAsync(
        long userId,
        long emailAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var emailAccount = await FindOwnedAccountAsync(userId, emailAccountId, cancellationToken);
        if (emailAccount.SenderAccount != null)
            await DisableSenderAsync(emailAccount.SenderAccount, cancellationToken);
        if (emailAccount.ReceivingAccount != null)
            await DisableReceivingAsync(emailAccount.ReceivingAccount, cancellationToken);

        await db
            .EmailAccountOAuthCredentials.Where(x => x.EmailAccountId == emailAccount.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true), cancellationToken);
        emailAccount.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteInvalidSenderCapabilitiesAsync(
        long userId,
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var accounts = await db
            .EmailAccounts.Include(x => x.SenderAccount)
            .Include(x => x.ReceivingAccount)
            .Where(x =>
                x.UserId == userId
                && x.EmailGroupId == emailGroupId
                && x.SenderAccount != null
                && x.SenderAccount.Status == SenderAccountStatus.Invalid
            )
            .ToListAsync(cancellationToken);
        foreach (var account in accounts)
        {
            await DisableSenderAsync(account.SenderAccount!, cancellationToken);
            if (account.ReceivingAccount == null)
            {
                account.IsDeleted = true;
                await db
                    .EmailAccountOAuthCredentials.Where(x => x.EmailAccountId == account.Id)
                    .ExecuteUpdateAsync(
                        x => x.SetProperty(y => y.IsDeleted, true),
                        cancellationToken
                    );
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<EmailAccount> QueryEmailAccounts(long userId) =>
        db.EmailAccounts.AsNoTracking().Where(x => x.UserId == userId);

    /// <summary>
    /// 在实体筛选完成后再构造读取模型，避免 EF Core 对 record 构造结果继续筛选时失去成员绑定。
    /// </summary>
    private IQueryable<EmailAccountDto> SelectDtos(IQueryable<EmailAccount> emailAccounts) =>
        emailAccounts.Select(x => new EmailAccountDto(
            x.Id,
            x.EmailGroupId,
            x.Email,
            x.Name,
            x.Description,
            x.Remark,
            x.SenderAccount == null
                ? null
                : new EmailAccountSenderCapabilityDto(
                    x.SenderAccount.Id,
                    x.SenderAccount.Protocol,
                    x.SenderAccount.Status,
                    x.SenderAccount.ValidationFailureReason,
                    x.SenderAccount.ProxyId,
                    x.SenderAccount.MaxSendCountPerDay,
                    x.SenderAccount.SentTotalToday,
                    x.SenderAccount.ReplyToEmails,
                    x.SenderAccount.Weight,
                    db.SenderAccountSmtpCredentials.Any(c =>
                        c.SenderAccountId == x.SenderAccount.Id
                    )
                ),
            x.ReceivingAccount == null
                ? null
                : new EmailAccountReceivingCapabilityDto(
                    x.ReceivingAccount.Id,
                    x.ReceivingAccount.Protocol,
                    x.ReceivingAccount.Status,
                    x.ReceivingAccount.ContentRetentionDays,
                    x.ReceivingAccount.LastConnectedAtUtc,
                    x.ReceivingAccount.LastError,
                    db.ReceivingAccountImapCredentials.Any(c =>
                        c.ReceivingAccountId == x.ReceivingAccount.Id
                    )
                ),
            x.OAuthCredential == null ? null : x.OAuthCredential.ApplicationSource,
            x.OAuthCredential != null
                && x.OAuthCredential.EncryptedRefreshToken != null
                && x.OAuthCredential.EncryptedRefreshToken != ""
        ));

    private async Task<EmailAccount> FindOwnedAccountAsync(
        long userId,
        long emailAccountId,
        CancellationToken cancellationToken
    ) =>
        await db
            .EmailAccounts.Include(x => x.SenderAccount)
            .Include(x => x.ReceivingAccount)
            .Include(x => x.OAuthCredential)
            .FirstOrDefaultAsync(
                x => x.Id == emailAccountId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮箱账户不存在");

    private async Task ApplyCapabilitiesAsync(
        EmailAccount emailAccount,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken
    )
    {
        var senderAccount = await ApplySenderCapabilityAsync(
            emailAccount,
            request,
            cancellationToken
        );
        var receivingAccount = await ApplyReceivingCapabilityAsync(
            emailAccount,
            request,
            cancellationToken
        );
        await db.SaveChangesAsync(cancellationToken);

        if (request.ConfigurationKind == EmailAccountConfigurationKind.Basic)
        {
            await ApplyBasicCredentialsAsync(
                senderAccount,
                receivingAccount,
                request,
                cancellationToken
            );
            if (emailAccount.OAuthCredential != null)
                emailAccount.OAuthCredential.IsDeleted = true;
            else
            {
                await db
                    .EmailAccountOAuthCredentials.Where(x => x.EmailAccountId == emailAccount.Id)
                    .ExecuteUpdateAsync(
                        x => x.SetProperty(y => y.IsDeleted, true),
                        cancellationToken
                    );
            }
            return;
        }

        var mustConfigureOAuth =
            emailAccount.OAuthCredential == null || request.MicrosoftGraphApplication != null;
        if (!mustConfigureOAuth)
            return;

        var application =
            request.MicrosoftGraphApplication ?? new MicrosoftGraphApplicationWriteDto();
        await credentialService.ConfigureMicrosoftOAuthAsync(
            emailAccount,
            new MicrosoftOAuthApplicationInput(
                application.ApplicationSource,
                application.TenantId,
                application.ClientId,
                application.ClientSecret
            ),
            cancellationToken
        );
    }

    private async Task<SenderAccount?> ApplySenderCapabilityAsync(
        EmailAccount emailAccount,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken
    )
    {
        if (!request.Sender.IsEnabled)
        {
            if (emailAccount.SenderAccount != null)
                await DisableSenderAsync(emailAccount.SenderAccount, cancellationToken);
            return null;
        }

        var protocol =
            request.ConfigurationKind == EmailAccountConfigurationKind.Basic
                ? SendingProtocol.Smtp
                : SendingProtocol.MicrosoftGraph;
        var authenticationMethod =
            protocol == SendingProtocol.Smtp
                ? AuthenticationMethod.Password
                : AuthenticationMethod.OAuth2;
        var senderAccount = await GetOrCreateSenderAsync(
            emailAccount,
            protocol,
            authenticationMethod,
            cancellationToken
        );
        senderAccount.ProxyId = protocol == SendingProtocol.Smtp ? request.Sender.ProxyId : null;
        senderAccount.MaxSendCountPerDay = Math.Max(0, request.Sender.MaxSendCountPerDay);
        senderAccount.ReplyToEmails = request.Sender.ReplyToEmails;
        senderAccount.Weight = Math.Max(1, request.Sender.Weight);
        return senderAccount;
    }

    private async Task<ReceivingAccount?> ApplyReceivingCapabilityAsync(
        EmailAccount emailAccount,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken
    )
    {
        if (!request.Receiving.IsEnabled)
        {
            if (emailAccount.ReceivingAccount != null)
                await DisableReceivingAsync(emailAccount.ReceivingAccount, cancellationToken);
            return null;
        }

        var protocol =
            request.ConfigurationKind == EmailAccountConfigurationKind.Basic
                ? ReceivingProtocol.Imap
                : ReceivingProtocol.MicrosoftGraph;
        var authenticationMethod =
            protocol == ReceivingProtocol.Imap
                ? AuthenticationMethod.Password
                : AuthenticationMethod.OAuth2;
        var receivingAccount = await GetOrCreateReceivingAsync(
            emailAccount,
            protocol,
            authenticationMethod,
            cancellationToken
        );
        receivingAccount.ContentRetentionDays = ValidateRetentionDays(
            request.Receiving.ContentRetentionDays
        );
        return receivingAccount;
    }

    private async Task ApplyBasicCredentialsAsync(
        SenderAccount? senderAccount,
        ReceivingAccount? receivingAccount,
        EmailAccountWriteDto request,
        CancellationToken cancellationToken
    )
    {
        if (senderAccount != null)
        {
            if (request.Sender.SmtpCredential != null)
            {
                var credential = request.Sender.SmtpCredential;
                await credentialService.SetSmtpCredentialAsync(
                    senderAccount,
                    new ProtocolCredentialInput(
                        credential.Host,
                        credential.Port,
                        credential.ConnectionSecurity,
                        credential.LoginName,
                        credential.Password
                    ),
                    cancellationToken
                );
            }
            else if (!await HasSmtpCredentialAsync(senderAccount.Id, cancellationToken))
            {
                throw new KnownException("启用 SMTP 发件设置时必须填写凭据");
            }
        }

        if (receivingAccount == null)
            return;
        if (request.Receiving.ImapCredential != null)
        {
            var credential = request.Receiving.ImapCredential;
            await credentialService.SetImapCredentialAsync(
                receivingAccount,
                new ProtocolCredentialInput(
                    credential.Host,
                    credential.Port,
                    credential.ConnectionSecurity,
                    credential.LoginName,
                    credential.Password
                ),
                cancellationToken
            );
            return;
        }

        if (!await HasImapCredentialAsync(receivingAccount.Id, cancellationToken))
            throw new KnownException("启用 IMAP 收件设置时必须填写凭据");
    }

    private async Task<SenderAccount> GetOrCreateSenderAsync(
        EmailAccount emailAccount,
        SendingProtocol protocol,
        AuthenticationMethod authenticationMethod,
        CancellationToken cancellationToken
    )
    {
        var senderAccount =
            emailAccount.SenderAccount
            ?? await db
                .SenderAccounts.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.EmailAccountId == emailAccount.Id, cancellationToken);
        if (senderAccount == null)
        {
            senderAccount = new SenderAccount { EmailAccountId = emailAccount.Id };
            db.SenderAccounts.Add(senderAccount);
        }

        if (senderAccount.Protocol != protocol)
            await DisableSmtpCredentialAsync(senderAccount.Id, cancellationToken);
        senderAccount.IsDeleted = false;
        senderAccount.Protocol = protocol;
        senderAccount.AuthenticationMethod = authenticationMethod;
        senderAccount.Status = SenderAccountStatus.Unverified;
        senderAccount.ValidationFailureReason = null;
        emailAccount.SenderAccount = senderAccount;
        return senderAccount;
    }

    private async Task<ReceivingAccount> GetOrCreateReceivingAsync(
        EmailAccount emailAccount,
        ReceivingProtocol protocol,
        AuthenticationMethod authenticationMethod,
        CancellationToken cancellationToken
    )
    {
        var receivingAccount =
            emailAccount.ReceivingAccount
            ?? await db
                .ReceivingAccounts.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.EmailAccountId == emailAccount.Id, cancellationToken);
        if (receivingAccount == null)
        {
            receivingAccount = new ReceivingAccount { EmailAccountId = emailAccount.Id };
            db.ReceivingAccounts.Add(receivingAccount);
        }

        if (receivingAccount.Protocol != protocol)
            await DisableImapCredentialAsync(receivingAccount.Id, cancellationToken);
        receivingAccount.IsDeleted = false;
        receivingAccount.Protocol = protocol;
        receivingAccount.AuthenticationMethod = authenticationMethod;
        receivingAccount.Status = ReceivingAccountStatus.Unverified;
        receivingAccount.LastError = null;
        emailAccount.ReceivingAccount = receivingAccount;
        return receivingAccount;
    }

    private async Task DisableSenderAsync(
        SenderAccount senderAccount,
        CancellationToken cancellationToken
    )
    {
        await DisableSmtpCredentialAsync(senderAccount.Id, cancellationToken);
        senderAccount.IsDeleted = true;
    }

    private async Task DisableReceivingAsync(
        ReceivingAccount receivingAccount,
        CancellationToken cancellationToken
    )
    {
        await DisableImapCredentialAsync(receivingAccount.Id, cancellationToken);
        receivingAccount.IsDeleted = true;
    }

    private Task<int> DisableSmtpCredentialAsync(
        long senderAccountId,
        CancellationToken cancellationToken
    ) =>
        db
            .SenderAccountSmtpCredentials.Where(x => x.SenderAccountId == senderAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true), cancellationToken);

    private Task<int> DisableImapCredentialAsync(
        long receivingAccountId,
        CancellationToken cancellationToken
    ) =>
        db
            .ReceivingAccountImapCredentials.Where(x => x.ReceivingAccountId == receivingAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true), cancellationToken);

    private Task<bool> HasSmtpCredentialAsync(
        long senderAccountId,
        CancellationToken cancellationToken
    ) =>
        db.SenderAccountSmtpCredentials.AnyAsync(
            x => x.SenderAccountId == senderAccountId,
            cancellationToken
        );

    private Task<bool> HasImapCredentialAsync(
        long receivingAccountId,
        CancellationToken cancellationToken
    ) =>
        db.ReceivingAccountImapCredentials.AnyAsync(
            x => x.ReceivingAccountId == receivingAccountId,
            cancellationToken
        );

    private async Task EnsureEmailAccountGroupAsync(
        long userId,
        long emailGroupId,
        CancellationToken cancellationToken
    )
    {
        if (
            !await db.EmailGroups.AnyAsync(
                x =>
                    x.Id == emailGroupId
                    && x.UserId == userId
                    && x.Category == EmailGroupCategory.EmailAccount,
                cancellationToken
            )
        )
            throw new KnownException("邮箱账户分组不存在");
    }

    private static void ValidateConfiguration(EmailAccountWriteDto request)
    {
        if (
            request.ConfigurationKind
            is not (
                EmailAccountConfigurationKind.Basic
                or EmailAccountConfigurationKind.MicrosoftGraph
            )
        )
            throw new KnownException("不支持的邮箱账户类型");
        if (!request.Sender.IsEnabled && !request.Receiving.IsEnabled)
            throw new KnownException("至少需要启用发件设置或收件设置");
    }

    private static int ValidateRetentionDays(int retentionDays)
    {
        if (retentionDays is < 1 or > 3650)
            throw new KnownException("邮件保留天数必须在 1 到 3650 之间");
        return retentionDays;
    }
}
