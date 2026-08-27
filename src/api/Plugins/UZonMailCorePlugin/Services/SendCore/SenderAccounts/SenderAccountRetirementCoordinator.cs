using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

/// <summary>
/// 协调发件箱退出运行池所需的持久化和关联任务清理。
/// </summary>
public interface ISenderAccountRetirementCoordinator
{
    /// <summary>
    /// 将发件箱移出运行池，并返回当前邮件的后续动作。
    /// </summary>
    Task<SenderAccountRetirementResult?> RetireAsync(
        SenderEmailAddress senderAccount,
        SendItemExecution? currentAttempt,
        DateTime groupTaskStartDate
    );
}

/// <inheritdoc />
public sealed class SenderAccountRetirementCoordinator(
    SenderAccountInvalidationPersistenceService invalidationPersistence,
    SenderAccountsManager senderAccountsManager,
    SenderAccountLinkedGroupCleanupService linkedGroupCleanup
) : ISenderAccountRetirementCoordinator, IScopedService<ISenderAccountRetirementCoordinator>
{
    /// <inheritdoc />
    public async Task<SenderAccountRetirementResult?> RetireAsync(
        SenderEmailAddress senderAccount,
        SendItemExecution? currentAttempt,
        DateTime groupTaskStartDate
    )
    {
        if (!senderAccount.ShouldDispose)
            return null;

        // 关联组是否仍有可用发件箱，必须在当前发件箱移出运行池后判断。
        await invalidationPersistence.PersistAsync(senderAccount);
        senderAccountsManager.RemoveSenderAccount(senderAccount, senderAccount.ErroredMessage);
        await linkedGroupCleanup.CleanupAsync(senderAccount, groupTaskStartDate);

        var descriptor = currentAttempt?.Descriptor;
        var canRetry =
            descriptor is { SenderAccountId: <= 0 }
            && senderAccountsManager.ExistValidSenderAccount(descriptor.SendingGroupId);
        return new SenderAccountRetirementResult(
            SenderAccountRetirementDecisionPolicy.Decide(descriptor, canRetry)
        );
    }
}
