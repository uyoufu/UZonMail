namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 账户的认证方式。
/// </summary>
public enum ImapAuthenticationType
{
    /// <summary>
    /// 使用用户名和加密密码认证。
    /// </summary>
    Password = 0,

    /// <summary>
    /// 使用 OAuth 2.0 访问令牌认证。
    /// </summary>
    OAuth2 = 1,
}

/// <summary>
/// OAuth 令牌所属的邮件服务提供商。
/// </summary>
public enum OAuthProviderType
{
    /// <summary>
    /// 使用自定义 OAuth 端点的提供商。
    /// </summary>
    Generic = 0,

    /// <summary>
    /// Google 邮箱服务。
    /// </summary>
    Google = 1,

    /// <summary>
    /// Microsoft 365 或 Outlook 邮箱服务。
    /// </summary>
    Microsoft = 2,
}

/// <summary>
/// IMAP 账户的可用状态。
/// </summary>
public enum ImapAccountStatus
{
    /// <summary>
    /// 账户可正常执行同步。
    /// </summary>
    Active = 0,

    /// <summary>
    /// 用户主动暂停同步。
    /// </summary>
    Paused = 1,

    /// <summary>
    /// 凭据无效或令牌已失效。
    /// </summary>
    AuthenticationFailed = 2,

    /// <summary>
    /// 无法与 IMAP 服务器建立连接。
    /// </summary>
    ConnectionFailed = 3,
}
