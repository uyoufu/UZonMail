using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

public sealed record SendRuntimeGroupSnapshot(
    long OrganizationId,
    long UserId,
    long SendingGroupId,
    int ReadyCount,
    int DelayedCount,
    int ActiveLeaseCount
);

public sealed record SendRuntimeOutboxSnapshot(
    OutboxKey Key,
    int ActiveLeaseCount,
    bool IsDraining
);

public sealed record SendRuntimeSnapshot(
    int RunningTasksCount,
    int ReadyCount,
    int DelayedCount,
    IReadOnlyList<SendRuntimeGroupSnapshot> Groups,
    IReadOnlyList<SendRuntimeOutboxSnapshot> Outboxes
);

public interface ISendRuntimeDiagnostics
{
    SendRuntimeSnapshot GetSnapshot();
}
