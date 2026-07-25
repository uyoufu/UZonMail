using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 在邮件和额度更新完成后清理正常退出运行池的发件箱。
/// </summary>
public sealed class OutboxRetirementHandler(IOutboxRetirementCoordinator retirementCoordinator)
    : AbstractSendingHandler
{
    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        if (context.OutboxRetirement != null)
            return HandlerResult.Skiped();

        var outbox = context.OutboxAddress;
        if (outbox == null || !outbox.ShouldDispose)
            return HandlerResult.Skiped();

        context.OutboxRetirement = await retirementCoordinator.RetireAsync(
            outbox,
            null,
            context.GroupTaskStartDate
        );
        return HandlerResult.Success();
    }
}
