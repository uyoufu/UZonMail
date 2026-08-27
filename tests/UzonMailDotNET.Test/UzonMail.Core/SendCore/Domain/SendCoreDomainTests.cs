using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Domain;

/// <summary>
/// 验证发送领域结果、快照派生属性和决策边界。
/// </summary>
[TestClass]
public sealed class SendCoreDomainTests
{
    [TestMethod]
    public void TransportResultFactories_CreateStableSuccessAndFailureValues()
    {
        var success = TransportResult.Success("receipt", "accepted");
        var failure = TransportResult.Failure(
            SendFailureKind.Network,
            "offline",
            errorCode: "socket"
        );

        Assert.IsTrue(success.IsSuccess);
        Assert.AreEqual(SendFailureKind.None, success.FailureKind);
        Assert.AreEqual("receipt", success.ReceiptId);
        Assert.IsFalse(failure.IsSuccess);
        Assert.AreEqual(SendFailureKind.Network, failure.FailureKind);
        Assert.AreEqual("socket", failure.ErrorCode);
    }

    [TestMethod]
    public void PreparedItem_UsesMessageProxyBeforeSenderAccountProxy()
    {
        var senderAccount = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            configure: entity => entity.ProxyId = 41
        );
        var senderAccountProxy = SendCoreTestEntityFactory.CreatePreparedItem(senderAccount);
        var messageProxy = SendCoreTestEntityFactory.CreatePreparedItem(
            senderAccount,
            configureItem: item => item.ProxyId = 52,
            configureSetting: setting => setting.MaxRetryCount = 7
        );

        Assert.AreEqual(41L, senderAccountProxy.EffectiveProxyId);
        Assert.AreEqual(52L, messageProxy.EffectiveProxyId);
        Assert.AreEqual(7, messageProxy.MaxRetryCount);
        Assert.HasCount(1, messageProxy.Recipients);
        Assert.IsEmpty(messageProxy.CC);
        Assert.IsEmpty(messageProxy.BCC);
    }

    [TestMethod]
    public void ValidationResults_ExposeTypedFailureAndSuccess()
    {
        var success = SendItemValidationResult.Valid();
        var failure = SendItemValidationResult.Invalid(
            SendItemValidationFailure.MissingBody,
            "missing"
        );

        Assert.IsTrue(success.IsValid);
        Assert.AreEqual(SendItemValidationFailure.None, success.Failure);
        Assert.IsFalse(failure.IsValid);
        Assert.AreEqual("missing", failure.Message);
    }

    [TestMethod]
    [DataRow(SendFailureKind.Transient)]
    [DataRow(SendFailureKind.Network)]
    [DataRow(SendFailureKind.Proxy)]
    [DataRow(SendFailureKind.Unknown)]
    public void RetryableFailures_RetryBeforeLimitAndFailAtLimit(SendFailureKind failureKind)
    {
        var retry = SendAttemptDecisionPolicy.Decide(
            TransportResult.Failure(failureKind, "temporary"),
            1,
            2,
            null
        );
        var failed = SendAttemptDecisionPolicy.Decide(
            TransportResult.Failure(failureKind, "temporary"),
            2,
            2,
            null
        );

        Assert.AreEqual(SendAttemptDisposition.Retry, retry.Disposition);
        Assert.IsFalse(retry.IsTerminal);
        Assert.AreEqual(SendAttemptDisposition.Failed, failed.Disposition);
        Assert.IsTrue(failed.IsTerminal);
    }

    [TestMethod]
    public void Execution_ExposesLeaseDescriptor()
    {
        var descriptor = new SendItemDescriptor(1, 10, 0, 2);
        var lease = new SendLease(
            Guid.CreateVersion7(),
            descriptor,
            new SenderAccountKey(30, 20),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        var execution = new SendItemExecution(
            lease,
            SendCoreTestEntityFactory.CreatePreparedItem()
        );

        Assert.AreSame(descriptor, execution.Descriptor);
    }

    [TestMethod]
    public void NetworkRouteResultFactories_UseTypedProxyFailure()
    {
        var route = new NetworkRoute(NetworkRouteKind.StaticProxy, "proxy-1", null);
        var success = NetworkRouteResolution.Success(route);
        var failure = NetworkRouteResolution.Failure("unavailable");

        Assert.IsTrue(success.IsSuccess);
        Assert.AreSame(route, success.Route);
        Assert.IsFalse(failure.IsSuccess);
        Assert.AreEqual(SendFailureKind.Proxy, failure.FailureKind);
    }
}
