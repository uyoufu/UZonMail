namespace UzonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 发件组的状态
    /// </summary>
    public enum SendingGroupStatus
    {
        /// <summary>
        /// 新建
        /// </summary>
        Created = 0,

        /// <summary>
        /// 计划发件
        /// </summary>
        [Obsolete("弃用，使用 type 表示计划发件")]
        Scheduled = 1,

        /// <summary>
        /// 发送中
        /// </summary>
        Sending = 2,

        /// <summary>
        /// 暂停
        /// </summary>
        Pause = 3,

        /// <summary>
        /// 停止
        /// </summary>
        Cancel = 4,

        /// <summary>
        /// 发送完成
        /// </summary>
        Finish = 5,

        /// <summary>
        /// 所有关联发件箱均达到当日额度，等待下一个 UTC 自然日自动恢复。
        /// </summary>
        WaitingForQuotaReset = 6,
    }
}
