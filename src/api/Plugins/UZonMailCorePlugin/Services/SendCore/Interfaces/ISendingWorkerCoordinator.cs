namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingWorkerCoordinator
    {
        void RegisterTenant(long userId, long organizationId);

        Task StartSendingAsync(CancellationToken cancellationToken = default);
    }
}
