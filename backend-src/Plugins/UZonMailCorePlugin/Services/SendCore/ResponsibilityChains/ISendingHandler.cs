using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public interface ISendingHandler : ITransientService
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
