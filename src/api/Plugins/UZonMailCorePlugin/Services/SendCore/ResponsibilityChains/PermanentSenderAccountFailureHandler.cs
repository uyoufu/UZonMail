using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 在提交当前邮件前协调 Transport 发现的永久发件箱失效。
/// </summary>
public sealed class PermanentSenderAccountFailureHandler(
    ISenderAccountRetirementCoordinator retirementCoordinator
) : AbstractSendingHandler
{
    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        if (context.TransportResult?.FailureKind != SendFailureKind.SenderAccountPermanent)
            return HandlerResult.Skiped();

        var senderAccount = context.SenderAccountAddress;
        if (senderAccount == null)
            return HandlerResult.Skiped();

        context.SenderAccountRetirement = await retirementCoordinator.RetireAsync(
            senderAccount,
            context.CurrentAttempt,
            context.GroupTaskStartDate
        );
        return HandlerResult.Success();
    }
}
