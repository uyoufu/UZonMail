namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 发件箱类型
    /// </summary>
    public enum OutboxType
    {
        /// <summary>
        /// Smtp 认证
        /// </summary>
        SMTP = 1,

        /// <summary>
        /// MsGraph OAuth 认证
        /// </summary>
        MsGraph,
    }
}
