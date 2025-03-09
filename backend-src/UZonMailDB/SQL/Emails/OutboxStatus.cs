namespace UZonMail.DB.SQL.Emails
{
    public enum OutboxStatus
    {
        /// <summary>
        /// 没有测试
        /// </summary>
        None,

        /// <summary>
        /// 有效
        /// </summary>
        Valid,

        /// <summary>
        /// 不可用
        /// </summary>
        Invalid,
    }
}
