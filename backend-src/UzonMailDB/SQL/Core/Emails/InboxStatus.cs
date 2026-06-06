namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 收件箱状态
    /// 用于标识是否有效
    /// </summary>
    public enum InboxStatus
    {
        /// <summary>
        /// 没有测试
        /// </summary>
        None,

        /// <summary>
        /// 不可用
        /// </summary>
        Invalid,

        /// <summary>
        /// 无法验证
        /// </summary>
        Unkown,

        /// <summary>
        /// 有效
        /// </summary>
        Valid = 200
    }
}
