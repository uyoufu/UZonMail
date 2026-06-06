namespace UZonMail.CorePlugin.SignalRHubs.SendEmail
{
    public class UserSendingProgressArg : SendingProgressArg
    {
        /// <summary>
        /// 在发任务总数
        /// </summary>
        public int TaskCount { get; set; }
    }
}
