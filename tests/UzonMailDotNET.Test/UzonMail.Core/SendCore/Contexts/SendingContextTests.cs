using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Contexts;

/// <summary>
/// 验证发送上下文的状态聚合和退出请求语义。
/// </summary>
[TestClass]
public sealed class SendingContextTests
{
    [TestMethod]
    public void HandlerResultFactories_PreserveStatusMessageAndChainState()
    {
        var normal = HandlerResult.Normal("normal");
        var skipped = HandlerResult.Skiped("skip", ChainStatus.BreakChain);
        var success = HandlerResult.Success("success");
        var failed = HandlerResult.Failed("failed", ChainStatus.ShouldExitTask);

        Assert.AreEqual(HandlerStatus.Normal, normal.HandlerStatus);
        Assert.AreEqual("normal", normal.Message);
        Assert.AreEqual(HandlerStatus.Skiped, skipped.HandlerStatus);
        Assert.AreEqual(ChainStatus.BreakChain, skipped.ChainStatus);
        Assert.AreEqual(HandlerStatus.Success, success.HandlerStatus);
        Assert.AreEqual(HandlerStatus.Failed, failed.HandlerStatus);
        Assert.AreEqual(ChainStatus.ShouldExitTask, failed.ChainStatus);
    }

    [TestMethod]
    public void Context_AggregatesFailuresAndExitRequests()
    {
        var context = new SendingContext(null!, null!, null!);
        var senderAccount = SendCoreTestEntityFactory.CreateSenderAccountAddress();

        Assert.AreSame(context, context.SetSenderAccount(senderAccount));
        Assert.AreSame(senderAccount, context.SenderAccountAddress);
        Assert.IsFalse(context.IsFailed());
        Assert.IsFalse(context.ShouldExitTask());

        context.HandleResults.Add(HandlerResult.Failed("failure"));
        Assert.IsTrue(context.IsFailed());

        context.HandleResults.Add(HandlerResult.Success(chainStatus: ChainStatus.ShouldExitTask));
        Assert.IsTrue(context.ShouldExitTask());
    }

    [TestMethod]
    public void RequestWorkerExit_IsSticky()
    {
        var context = new SendingContext(null!, null!, null!);

        context.RequestWorkerExit();
        context.HandleResults.Add(HandlerResult.Success());

        Assert.IsTrue(context.ShouldExitTask());
    }
}
