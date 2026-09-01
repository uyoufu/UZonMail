using UzonMail.DB.SQL.Core.EmailReceiving;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

public interface IReceivingSynchronizationService
{
    Task<ReceivingSynchronizationResult> SynchronizeAsync(
        long userId,
        long receivingAccountId,
        ImapSyncTrigger trigger,
        CancellationToken cancellationToken = default
    );
}

public sealed record ReceivingSynchronizationResult(
    long SyncRunId,
    int MessagesDiscovered,
    int MessagesCreated,
    int MessagesUpdated
);
