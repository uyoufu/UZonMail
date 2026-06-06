namespace UZonMail.DB.SQL.Core.EmailSending
{
    /// <summary>
    /// 发件组的类型
    /// </summary>
    public enum SendingGroupType
    {
        /// <summary>
        /// 即时发送
        /// </summary>
        Instant,

        /// <summary>
        /// 计划发送
        /// </summary>
        Scheduled
    }
}
