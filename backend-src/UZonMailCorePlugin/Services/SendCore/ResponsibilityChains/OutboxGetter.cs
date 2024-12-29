using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Outboxes;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 获取发件箱
    /// </summary>
    /// <param name="container"></param>
    public class OutboxGetter(OutboxesPoolList container) : AbstractSendingHandler
    {
        protected override async Task HandleCore(SendingContext context)
        {
            // 获取发件箱
            var address = container.GetOutbox();
            // 保存到 context 中
            context.OutboxAddress = address;

            // 如果获取失败，则停止线程
            if (address == null)
            {
                context.Status |= ContextStatus.Fail;
                context.Status |= ContextStatus.ShouldExitThread;
            }

            return;
        }
    }
}
