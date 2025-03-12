using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.SignalRHubs.SendEmail
{
    /// <summary>
    /// 发送项进度参数
    /// </summary>
    /// <param name="sendingItem"></param>
    public class SendingItemStatusChangedArg(SendingItem sendingItem)
    {
        /// <summary>
        /// 邮件id
        /// </summary>
        public long SendingItemId { get; private set; } = sendingItem.Id;

        /// <summary>
        /// 状态
        /// </summary>
        public SendingItemStatus Status { get; set; } = sendingItem.Status;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int TriedCount { get; private set; } = sendingItem.TriedCount;

        /// <summary>
        /// 结果描述
        /// </summary>
        public string? SendResult { get; private set; } = sendingItem.SendResult;

        /// <summary>
        /// 发件箱
        /// </summary>
        public string FromEmail { get; private set; } = sendingItem.FromEmail;

        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime SendDate { get; private set; } = sendingItem.SendDate;

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<EmailAddress> Inboxes { get; private set; } = sendingItem.Inboxes;

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; private set; } = sendingItem.Subject;
    }
}
