using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Reading;

public sealed class EfSendItemPageSource(SqlContext db)
    : ISendItemPageSource,
        IScopedService<ISendItemPageSource>
{
    public async Task<IReadOnlyList<SendItemDescriptor>> ReadPageAsync(
        SendItemPageRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = db
            .SendingItems.AsNoTracking()
            .Where(x => x.SendingGroupId == request.SendingGroupId)
            .Where(x =>
                x.Status == SendingItemStatus.Created
                || x.Status == SendingItemStatus.Failed
                || (request.IncludePending && x.Status == SendingItemStatus.Pending)
            )
            .Where(x =>
                x.OutBoxId > request.Cursor.OutboxId
                || (x.OutBoxId == request.Cursor.OutboxId && x.Id > request.Cursor.Id)
            );

        if (request.SelectedItemIds is { Count: > 0 })
            query = query.Where(x => request.SelectedItemIds.Contains(x.Id));

        return await query
            .OrderBy(x => x.OutBoxId)
            .ThenBy(x => x.Id)
            .Select(x => new SendItemDescriptor(x.Id, x.SendingGroupId, x.OutBoxId, x.TriedCount))
            .Take(request.Take)
            .ToListAsync(cancellationToken);
    }
}
