using UZonMail.Core.Services.SendCore.Contexts;

namespace UZonMail.Core.Services.SendCore.Event.Commands
{
    public class StartSendingCommand : GenericCommand<int>
    {
        public StartSendingCommand(int count, SendingContext? scopeServices = null) : base(CommandType.StartSending, scopeServices, count)
        {
        }
    }
}
