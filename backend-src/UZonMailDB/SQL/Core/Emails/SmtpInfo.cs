using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// Smtp配置
    /// </summary>
    public class SmtpInfo : SqlId
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 对应的 smtp 服务器
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 安全协议
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; }

        /// <summary>
        /// 是否需要身份验证
        /// </summary>
        public bool EnableSSL { get; set; } = false;
    }

    [Flags]
    public enum SecurityProtocol
    {
        None = 1,
        SSL = 2,
        TLS = 4,
        StartTLS = 8
    }
}
