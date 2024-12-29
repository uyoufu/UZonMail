namespace UZonMail.Core.SignalRHubs.SendEmail
{
    public class EmailSendingStatus
    {
        public string Inbox { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// 0 发送失败；1 等待改善；2 开始发送；3 重新发送；4 发送成功
        /// </summary>
        public EmailSendingStatusEnum Status { get; set; }
    }

    public enum EmailSendingStatusEnum
    {
        Failed = 0,
        Waiting = 1,
        Sending = 2,
        Resending = 3,
        Success = 4
    }
}
