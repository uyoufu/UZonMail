using UZonMail.CorePlugin.Services.SendCore.Contexts;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingPipeline
    {
        Task Handle(SendingContext context);
    }
}
