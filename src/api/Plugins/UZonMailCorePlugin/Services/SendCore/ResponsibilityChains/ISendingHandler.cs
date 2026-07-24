using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public interface ISendingHandler : IScopedService
    {
        Task<IHandlerResult> Execute(SendingContext context);
    }
}
