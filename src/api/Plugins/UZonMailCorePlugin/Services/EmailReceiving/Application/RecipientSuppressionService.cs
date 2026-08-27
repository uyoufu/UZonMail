using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailReceiving.Application;

public sealed class RecipientSuppressionService(SqlContext db)
    : IRecipientSuppressionService,
        IScopedService<IRecipientSuppressionService>
{
    public async Task<IReadOnlySet<string>> GetSuppressedEmailsAsync(
        long organizationId,
        IEnumerable<string> emails,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmails = emails.Select(NormalizeEmail).Distinct().ToArray();
        if (normalizedEmails.Length == 0)
            return new HashSet<string>();

        var suppressedEmails = await db
            .RecipientSuppressions.AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId
                && x.IsActive
                && normalizedEmails.Contains(x.Email)
            )
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);
        return suppressedEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SuppressAsync(
        long organizationId,
        string email,
        RecipientSuppressionReason reason,
        long actorUserId,
        string? reasonDetail = null,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = NormalizeEmail(email);
        var suppression = await db.RecipientSuppressions.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Email == normalizedEmail,
            cancellationToken
        );
        if (suppression == null)
        {
            suppression = new RecipientSuppression
            {
                OrganizationId = organizationId,
                Email = normalizedEmail,
            };
            db.RecipientSuppressions.Add(suppression);
        }

        suppression.Reason = reason;
        suppression.ReasonDetail = reasonDetail;
        suppression.IsActive = true;
        suppression.CreatedByUserId = actorUserId;
        suppression.SuppressedAtUtc = DateTime.UtcNow;
        suppression.ReleasedByUserId = null;
        suppression.ReleasedAtUtc = null;
        suppression.ReleaseReason = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseAsync(
        long organizationId,
        string email,
        long actorUserId,
        string releaseReason,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(releaseReason))
            throw new KnownException("解除停发必须填写原因");
        var normalizedEmail = NormalizeEmail(email);
        var suppression =
            await db.RecipientSuppressions.FirstOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Email == normalizedEmail && x.IsActive,
                cancellationToken
            ) ?? throw new KnownException("停发记录不存在");
        suppression.IsActive = false;
        suppression.ReleasedByUserId = actorUserId;
        suppression.ReleasedAtUtc = DateTime.UtcNow;
        suppression.ReleaseReason = releaseReason.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new KnownException("邮箱地址不能为空");
        return email.Trim().ToLowerInvariant();
    }
}
