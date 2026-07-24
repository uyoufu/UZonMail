namespace UzonMail.CorePlugin.Services.SendCore.Interfaces
{
    public interface ISendingTasksManager
    {
        int RunningTasksCount { get; }

        /// <summary>
        /// 开始发送
        /// </summary>
        Task StartSendingAsync(CancellationToken cancellationToken = default);
    }
}
