using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public interface ISendingHandler : IScopedService
    {
        /// <summary>
        /// 设置下一个处理者
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        ISendingHandler SetNext(ISendingHandler next);

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Handle(SendingContext context);
    }
}
