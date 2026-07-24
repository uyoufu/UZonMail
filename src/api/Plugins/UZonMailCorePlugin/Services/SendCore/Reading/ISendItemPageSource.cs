using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Reading;

public sealed record SendItemPageRequest(
    long SendingGroupId,
    SendItemCursor Cursor,
    int Take,
    IReadOnlyCollection<long>? SelectedItemIds,
    bool IncludePending
);

public interface ISendItemPageSource : IScopedService<ISendItemPageSource>
{
    Task<IReadOnlyList<SendItemDescriptor>> ReadPageAsync(
        SendItemPageRequest request,
        CancellationToken cancellationToken
    );
}
