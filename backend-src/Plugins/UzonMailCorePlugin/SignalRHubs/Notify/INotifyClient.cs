namespace UZonMail.CorePlugin.SignalRHubs.Notify
{
    public interface INotifyClient
    {
        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Notify(NotifyMessage message);
    }
}
