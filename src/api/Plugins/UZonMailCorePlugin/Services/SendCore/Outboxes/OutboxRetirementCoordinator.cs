using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Outboxes;

/// <summary>
/// 协调发件箱退出运行池所需的持久化和关联任务清理。
/// </summary>
public interface IOutboxRetirementCoordinator
{
    /// <summary>
    /// 将发件箱移出运行池，并返回当前邮件的后续动作。
    /// </summary>
    Task<OutboxRetirementResult?> RetireAsync(
        OutboxEmailAddress outbox,
        SendItemExecution? currentAttempt,
        DateTime groupTaskStartDate
    );
}

/// <inheritdoc />
public sealed class OutboxRetirementCoordinator(
    OutboxInvalidationPersistenceService invalidationPersistence,
    OutboxesManager outboxesManager,
    OutboxLinkedGroupCleanupService linkedGroupCleanup
) : IOutboxRetirementCoordinator, IScopedService<IOutboxRetirementCoordinator>
{
    /// <inheritdoc />
    public async Task<OutboxRetirementResult?> RetireAsync(
        OutboxEmailAddress outbox,
        SendItemExecution? currentAttempt,
        DateTime groupTaskStartDate
    )
    {
        if (!outbox.ShouldDispose)
            return null;

        // 关联组是否仍有可用发件箱，必须在当前发件箱移出运行池后判断。
        await invalidationPersistence.PersistAsync(outbox);
        outboxesManager.RemoveOutbox(outbox, outbox.ErroredMessage);
        await linkedGroupCleanup.CleanupAsync(outbox, groupTaskStartDate);

        var descriptor = currentAttempt?.Descriptor;
        var canRetry =
            descriptor is { OutboxId: <= 0 }
            && outboxesManager.ExistValidOutbox(descriptor.SendingGroupId);
        return new OutboxRetirementResult(
            OutboxRetirementDecisionPolicy.Decide(descriptor, canRetry)
        );
    }
}
