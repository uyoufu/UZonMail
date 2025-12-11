namespace UZonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 发件组的状态
    /// </summary>
    public enum SendingGroupStatus
    {
        /// <summary>
        /// 新建
        /// </summary>
        Created,

        /// <summary>
        /// 计划发件
        /// </summary>
        [Obsolete("弃用，使用 type 表示计划发件")]
        Scheduled,

        /// <summary>
        /// 发送中
        /// </summary>
        Sending,

        /// <summary>
        /// 暂停
        /// </summary>
        Pause,

        /// <summary>
        /// 停止
        /// </summary>
        Cancel,

        /// <summary>
        /// 发送完成
        /// </summary>
        Finish,
    }
}
