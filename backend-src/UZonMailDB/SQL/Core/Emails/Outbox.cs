namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// 发件箱
    /// </summary>
    public class Outbox : EmailBox
    {
        //public OutboxAuthType AuthType { get; set; } = OutboxAuthType.Credential;

        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string SmtpHost { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Smtp 的用户名
        /// 与 smtp 的发件邮箱可能不一致
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// smtp 密码或者 oauth 的 secrete，需要加密保存
        /// smpt 密码 = 原始密码  > aes (md5 作为 key)
        /// </summary>
        public string Password { get; set; }

        ///// <summary>
        ///// 使用 OAuth 2.0 认证时的 Client Id
        ///// </summary>
        //public string? ClientId { get; set; }

        ///// <summary>
        ///// 使用 OAuth 2.0 认证时的 Tenant Id
        ///// </summary>
        //public string? TenantId { get; set; }

        /// <summary>
        /// 是否启用 SSL
        /// </summary>
        public bool EnableSSL { get; set; } = true;

        /// <summary>
        /// 代理 Id
        /// </summary>
        public long ProxyId { get; set; }

        /// <summary>
        /// 单日最大发送数量
        /// 为 0 时表示不限制
        /// </summary>
        public int MaxSendCountPerDay { get; set; }

        /// <summary>
        /// 当前已发送数量
        /// </summary>
        public int SentTotalToday { get; set; }

        /// <summary>
        /// 回复邮箱
        /// 多个邮箱地址使用逗号分隔
        /// </summary>
        public string? ReplyToEmails { get; set; }

        /// <summary>
        /// 发件权重
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 是否有效
        /// 必须通过验证后，才能正常使用
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 状态
        /// 用于向前端展示发件箱是否可用
        /// </summary>
        public OutboxStatus Status { get; set; }

        /// <summary>
        /// 验证失败原因
        /// 可选是为了兼容老版本
        /// </summary>
        public string? ValidFailReason { get; set; }


        public static bool IsExchangeEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            var domain = email.Trim().Split("@").Last().ToLower();
            return new List<string>() { "outlook.com", "hotmail.com" }.Contains(domain);
        }
    }
}
