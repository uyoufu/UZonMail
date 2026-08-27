using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 在邮件和额度更新完成后清理正常退出运行池的发件箱。
/// </summary>
public sealed class SenderAccountRetirementHandler(
    ISenderAccountRetirementCoordinator retirementCoordinator
) : AbstractSendingHandler
{
    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        if (context.SenderAccountRetirement != null)
            return HandlerResult.Skiped();

        var senderAccount = context.SenderAccountAddress;
        if (senderAccount == null || !senderAccount.ShouldDispose)
            return HandlerResult.Skiped();

        context.SenderAccountRetirement = await retirementCoordinator.RetireAsync(
            senderAccount,
            null,
            context.GroupTaskStartDate
        );
        return HandlerResult.Success();
    }
}
