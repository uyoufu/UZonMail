using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Emails;

/// <summary>
/// 管理发送和接收能力共享的邮箱身份。
/// </summary>
public sealed class EmailAccountService(SqlContext db) : IScopedService
{
    public async Task<EmailAccount> GetOrCreateAsync(
        long userId,
        long organizationId,
        string email,
        string? name,
        string? description,
        string? remark,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = NormalizeEmail(email);
        var account = await db.EmailAccounts.FirstOrDefaultAsync(
            x => x.UserId == userId && x.NormalizedEmail == normalizedEmail,
            cancellationToken
        );
        if (account == null)
        {
            account = new EmailAccount
            {
                UserId = userId,
                OrganizationId = organizationId,
                Email = email.Trim(),
            };
            db.EmailAccounts.Add(account);
        }

        account.Name = name;
        account.Description = description;
        account.Remark = remark;
        return account;
    }

    public static string NormalizeEmail(string email)
    {
        if (
            string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email.Trim(), out var parsed)
        )
            throw new KnownException("邮箱地址格式不正确");
        if (!string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new KnownException("邮箱地址必须是单一邮件地址");
        return parsed.Address.ToLowerInvariant();
    }
}
