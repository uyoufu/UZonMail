namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingWorkerCoordinator
    {
        Task StartSendingAsync(CancellationToken cancellationToken = default);
    }
}
