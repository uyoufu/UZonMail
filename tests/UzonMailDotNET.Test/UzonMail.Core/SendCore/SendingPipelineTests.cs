using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证发送责任链的顺序、结果聚合和中断语义。
/// </summary>
[TestClass]
public sealed class SendingPipelineTests
{
    [TestMethod]
    public async Task Handle_EmptyPipelineRecordsFailure()
    {
        var context = new SendingContext(null!, null!, null!);

        await new SendingPipeline([]).Handle(context);

        Assert.HasCount(2, context.HandleResults);
        Assert.AreEqual(HandlerStatus.Failed, context.HandleResults[1].HandlerStatus);
    }

    [TestMethod]
    public async Task Handle_ExecutesHandlersInOrderAndCollectsResults()
    {
        var calls = new List<int>();
        var pipeline = new SendingPipeline(
            [
                new StubHandler(1, calls, HandlerResult.Normal("first")),
                new StubHandler(2, calls, HandlerResult.Success("second")),
            ]
        );
        var context = new SendingContext(null!, null!, null!);

        await pipeline.Handle(context);

        CollectionAssert.AreEqual(new[] { 1, 2 }, calls);
        Assert.HasCount(3, context.HandleResults);
        Assert.AreEqual("second", context.HandleResults[2].Message);
    }

    [TestMethod]
    [DataRow(ChainStatus.BreakChain)]
    [DataRow(ChainStatus.ShouldExitTask)]
    public async Task Handle_StopsAfterTerminalChainResult(ChainStatus terminalStatus)
    {
        var calls = new List<int>();
        var pipeline = new SendingPipeline(
            [
                new StubHandler(1, calls, HandlerResult.Success(chainStatus: terminalStatus)),
                new StubHandler(2, calls, HandlerResult.Success()),
            ]
        );

        await pipeline.Handle(new SendingContext(null!, null!, null!));

        CollectionAssert.AreEqual(new[] { 1 }, calls);
    }

    private sealed class StubHandler(
        int sequence,
        List<int> calls,
        IHandlerResult result
    ) : ISendingHandler
    {
        public Task<IHandlerResult> Execute(SendingContext context)
        {
            calls.Add(sequence);
            return Task.FromResult(result);
        }
    }
}
