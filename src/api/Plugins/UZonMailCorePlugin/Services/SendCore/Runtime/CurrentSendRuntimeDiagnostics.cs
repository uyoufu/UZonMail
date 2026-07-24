using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

public sealed class CurrentSendRuntimeDiagnostics(
    UserGroupTasksPools groupPools,
    OutboxesManager outboxes,
    ISendingTasksManager tasksManager
) : ISendRuntimeDiagnostics, ISingletonService<ISendRuntimeDiagnostics>
{
    public SendRuntimeSnapshot GetSnapshot()
    {
        var groups = groupPools
            .Values.SelectMany(pool =>
                pool.GetTasks()
                    .Select(task => new SendRuntimeGroupSnapshot(
                        0,
                        pool.UserId,
                        task.SendingGroupId,
                        task.ReadyCount,
                        task.DelayedCount,
                        task.ActiveCount
                    ))
            )
            .ToList();
        var outboxSnapshots = outboxes
            .Values.Select(outbox => new SendRuntimeOutboxSnapshot(
                new OutboxKey(outbox.UserId, outbox.Id),
                outbox.IsRunningInTask ? 1 : 0,
                outbox.ShouldDispose
            ))
            .ToList();
        return new SendRuntimeSnapshot(
            tasksManager.RunningTasksCount,
            groups.Sum(x => x.ReadyCount),
            groups.Sum(x => x.DelayedCount),
            groups,
            outboxSnapshots
        );
    }
}
