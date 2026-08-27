using UzonMail.DB.SQL.Core.EmailReceiving;

namespace UzonMail.CorePlugin.Services.EmailReceiving.Application;

public interface IRecipientSuppressionService
{
    Task<IReadOnlySet<string>> GetSuppressedEmailsAsync(
        long organizationId,
        IEnumerable<string> emails,
        CancellationToken cancellationToken = default
    );

    Task SuppressAsync(
        long organizationId,
        string email,
        RecipientSuppressionReason reason,
        long actorUserId,
        string? reasonDetail = null,
        CancellationToken cancellationToken = default
    );

    Task ReleaseAsync(
        long organizationId,
        string email,
        long actorUserId,
        string releaseReason,
        CancellationToken cancellationToken = default
    );
}
