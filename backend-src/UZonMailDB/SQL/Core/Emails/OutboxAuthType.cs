namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 发件箱授权验证类型
    /// </summary>
    public enum OutboxAuthType
    {
        // 不需要授权验证
        None,

        /// <summary>
        /// 凭证授权
        /// </summary>
        Credential,

        /// <summary>
        /// OAuth 2.0 授权
        /// </summary>
        OAuth2,
    }
}
