using UzonMail.CorePlugin.Services.SendCore.Contexts;

namespace UzonMail.CorePlugin.Services.SendCore.Event.Commands
{
    public class StartSendingCommand : GenericCommand<int>
    {
        public StartSendingCommand(int count, SendingContext? scopeServices = null)
            : base(CommandType.StartSending, scopeServices, count) { }
    }
}
