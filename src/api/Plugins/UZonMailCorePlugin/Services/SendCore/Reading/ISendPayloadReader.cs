using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Reading;

public interface ISendPayloadReader
{
    Task<SendingItem?> ReadAsync(
        SqlContext db,
        long sendingItemId,
        CancellationToken cancellationToken = default
    );
}

public sealed class EfSendPayloadReader : ISendPayloadReader, ISingletonService<ISendPayloadReader>
{
    public Task<SendingItem?> ReadAsync(
        SqlContext db,
        long sendingItemId,
        CancellationToken cancellationToken = default
    )
    {
        return db
            .SendingItems.AsNoTracking()
            .Where(x => x.Id == sendingItemId)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
