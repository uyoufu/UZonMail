using UzonMail.CorePlugin.Services.SendCore.Contexts;

namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingPipeline
    {
        Task Handle(SendingContext context);
    }
}
