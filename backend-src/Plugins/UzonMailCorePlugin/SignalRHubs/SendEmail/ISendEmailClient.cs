namespace UZonMail.CorePlugin.SignalRHubs.SendEmail
{
    public interface ISendEmailClient
    {
        /// <summary>
        /// 单个邮件组发送进度
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task SendingGroupProgressChanged(SendingGroupProgressArg arg);

        /// <summary>
        /// 单个邮件组发送进度
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task SendingItemStatusChanged(SendingItemStatusChangedArg arg);

        /// <summary>
        /// 邮件发送错误
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendError(string message);
    }
}
