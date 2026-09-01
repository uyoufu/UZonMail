using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.MailConversations;

public sealed class ReceivingAccountQueryService(SqlContext db) : IScopedService
{
    public Task<List<ReceivingAccountSummaryDto>> GetAsync(
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        db
            .EmailAccounts.AsNoTracking()
            .Where(x => x.UserId == userId && x.SenderAccount != null)
            .OrderBy(x => x.Email)
            .Select(x =>
                x.ReceivingAccount == null
                    ? new ReceivingAccountSummaryDto(
                        x.Id,
                        0,
                        x.Email,
                        x.Name,
                        ReceivingProtocol.Imap,
                        ReceivingAccountStatus.ConfigurationRequired,
                        null,
                        "IMAP 收件未配置",
                        true
                    )
                    : new ReceivingAccountSummaryDto(
                        x.Id,
                        x.ReceivingAccount.Id,
                        x.Email,
                        x.Name,
                        x.ReceivingAccount.Protocol,
                        x.ReceivingAccount.Status,
                        x.ReceivingAccount.LastSuccessfulSyncAtUtc,
                        x.ReceivingAccount.LastError,
                        x.ReceivingAccount.Protocol == ReceivingProtocol.Imap
                    )
            )
            .ToListAsync(cancellationToken);
}
