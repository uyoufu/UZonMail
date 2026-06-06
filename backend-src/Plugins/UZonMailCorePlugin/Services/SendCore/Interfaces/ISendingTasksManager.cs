namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingTasksManager
    {
        /// <summary>
        /// 开始发送
        /// </summary>
        Task StartSendingAsync(CancellationToken cancellationToken = default);
    }
}
