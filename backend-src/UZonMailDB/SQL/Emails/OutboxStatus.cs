namespace UZonMail.DB.SQL.Emails
{
    public enum OutboxStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

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
