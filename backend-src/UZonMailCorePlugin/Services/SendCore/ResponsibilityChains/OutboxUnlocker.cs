using UZonMail.Core.Services.SendCore.Contexts;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件箱取消锁定
    /// </summary>
    public class OutboxUnlocker : AbstractSendingHandler
    {
        protected override async Task HandleCore(SendingContext context)
        {
            var outbox = context.OutboxAddress;
            if (outbox == null) return;
            outbox.UnlockUsing();
        }
    }
}
