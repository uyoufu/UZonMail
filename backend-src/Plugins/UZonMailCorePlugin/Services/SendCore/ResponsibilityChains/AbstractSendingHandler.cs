using System.Reflection.Metadata;
using Org.BouncyCastle.Asn1.Ocsp;
using UZonMail.CorePlugin.Services.SendCore.Contexts;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 不要在子类中保存状态，职责链模式的处理者应该是无状态的
    /// </summary>
    public abstract class AbstractSendingHandler : ISendingHandler
    {
        private ISendingHandler? _nextHandler;

        /// <summary>
        /// 职责链的处理方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Handle(SendingContext context)
        {
            // 触发当前处理者的处理方法
            var handleResult = await HandleCore(context);
            context.HandleResults.Add(handleResult);

            // 调用下一个处理者
            await this.Next(context);
        }

        public ISendingHandler SetNext(ISendingHandler handler)
        {
            this._nextHandler = handler;
            return handler;
        }

        protected async Task Next(SendingContext context)
        {
            if (this._nextHandler != null)
            {
                await this._nextHandler.Handle(context);
            }
        }

        protected abstract Task<IHandlerResult> HandleCore(SendingContext context);
    }
}
