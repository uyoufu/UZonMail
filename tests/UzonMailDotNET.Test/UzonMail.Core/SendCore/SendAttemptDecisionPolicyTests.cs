using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SendAttemptDecisionPolicyTests
{
    [TestMethod]
    public void Success_IsTerminalAndPreservesReceiptId()
    {
        var decision = SendAttemptDecisionPolicy.Decide(
            TransportResult.Success("receipt-id"),
            0,
            3,
            null
        );

        Assert.AreEqual(SendAttemptDisposition.Succeeded, decision.Disposition);
        Assert.AreEqual("receipt-id", decision.ReceiptId);
        Assert.IsTrue(decision.IsTerminal);
    }

    [TestMethod]
    [DataRow(SendFailureKind.RecipientPermanent)]
    [DataRow(SendFailureKind.HardBounce)]
    [DataRow(SendFailureKind.MessagePermanent)]
    [DataRow(SendFailureKind.LocalData)]
    public void PermanentItemFailure_FailsWithoutRetry(SendFailureKind failureKind)
    {
        var decision = Decide(failureKind, triedCount: 0, maxRetryCount: 3);
        Assert.AreEqual(SendAttemptDisposition.Failed, decision.Disposition);
    }

    [TestMethod]
    public void OutboxPermanent_RetriesOnlyWhenAnotherSharedOutboxExists()
    {
        var retry = SendAttemptDecisionPolicy.Decide(
            TransportResult.Failure(SendFailureKind.OutboxPermanent, "invalid"),
            0,
            3,
            new OutboxRetirementResult(CurrentSendItemDisposition.RetryWithAnotherOutbox)
        );
        var fail = SendAttemptDecisionPolicy.Decide(
            TransportResult.Failure(SendFailureKind.OutboxPermanent, "invalid"),
            0,
            3,
            new OutboxRetirementResult(CurrentSendItemDisposition.Fail)
        );

        Assert.AreEqual(SendAttemptDisposition.Retry, retry.Disposition);
        Assert.AreEqual(SendAttemptDisposition.Failed, fail.Disposition);
    }

    [TestMethod]
    public void RetryableFailure_FailsAfterRetryLimit()
    {
        Assert.AreEqual(
            SendAttemptDisposition.Retry,
            Decide(SendFailureKind.Network, 1, 3).Disposition
        );
        Assert.AreEqual(
            SendAttemptDisposition.Failed,
            Decide(SendFailureKind.Network, 3, 3).Disposition
        );
    }

    [TestMethod]
    public void Cancelled_ReleasesWithoutConsumingRetry()
    {
        var decision = Decide(SendFailureKind.Cancelled, 3, 3);
        Assert.AreEqual(SendAttemptDisposition.Release, decision.Disposition);
        Assert.IsFalse(decision.IsTerminal);
    }

    private static SendAttemptDecision Decide(
        SendFailureKind failureKind,
        int triedCount,
        int maxRetryCount
    ) =>
        SendAttemptDecisionPolicy.Decide(
            TransportResult.Failure(failureKind, "error"),
            triedCount,
            maxRetryCount,
            null
        );
}
