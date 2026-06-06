namespace UZonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 邮件状态
    /// </summary>
    public enum SendingItemStatus
    {
        /// <summary>
        /// 发送失败
        /// </summary>
        Failed,

        /// <summary>
        /// 无效
        /// </summary>
        Invalid,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancel,

        /// <summary>
        /// 初始状态
        /// </summary>
        Created,

        /// <summary>
        /// 等待发件中
        /// </summary>
        Pending,

        /// <summary>
        /// 发送状态
        /// </summary>
        Sending,

        /// <summary>
        /// 发送成功
        /// </summary>
        Success,

        /// <summary>
        /// 已读
        /// </summary>
        Read,

        /// <summary>
        /// 已取消订阅
        /// </summary>
        Unsubscribed,
    }
}
