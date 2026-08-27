using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

public sealed class CurrentSendRuntimeDiagnostics(
    UserGroupTasksPools groupPools,
    SenderAccountsManager senderAccounts,
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
        var senderAccountSnapshots = senderAccounts
            .Values.Select(senderAccount => new SendRuntimeSenderAccountSnapshot(
                new SenderAccountKey(senderAccount.UserId, senderAccount.Id),
                senderAccount.IsRunningInTask ? 1 : 0,
                senderAccount.ShouldDispose
            ))
            .ToList();
        return new SendRuntimeSnapshot(
            tasksManager.RunningTasksCount,
            groups.Sum(x => x.ReadyCount),
            groups.Sum(x => x.DelayedCount),
            groups,
            senderAccountSnapshots
        );
    }
}
