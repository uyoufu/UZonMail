using UzonMail.CorePlugin.Services.SendCore.Contexts;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 不要在子类中保存状态，职责链模式的处理者应该是无状态的
    /// </summary>
    public abstract class AbstractSendingHandler : ISendingHandler
    {
        public Task<IHandlerResult> Execute(SendingContext context)
        {
            return HandleCore(context);
        }

        protected abstract Task<IHandlerResult> HandleCore(SendingContext context);
    }
}
