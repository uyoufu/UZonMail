using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 在提交当前邮件前协调 Transport 发现的永久发件箱失效。
/// </summary>
public sealed class PermanentOutboxFailureHandler(
    IOutboxRetirementCoordinator retirementCoordinator
) : AbstractSendingHandler
{
    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        if (context.TransportResult?.FailureKind != SendFailureKind.OutboxPermanent)
            return HandlerResult.Skiped();

        var outbox = context.OutboxAddress;
        if (outbox == null)
            return HandlerResult.Skiped();

        context.OutboxRetirement = await retirementCoordinator.RetireAsync(
            outbox,
            context.CurrentAttempt,
            context.GroupTaskStartDate
        );
        return HandlerResult.Success();
    }
}
