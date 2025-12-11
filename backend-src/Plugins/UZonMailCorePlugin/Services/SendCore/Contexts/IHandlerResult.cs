namespace UZonMail.CorePlugin.Services.SendCore.Contexts
{
    public interface IHandlerResult
    {
        HandlerStatus HandlerStatus { get; }

        ChainStatus ChainStatus { get; }

        string Message { get; }
    }
}
