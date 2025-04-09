using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// Smtp配置
    /// </summary>
    public class SmtpConfig : SqlId
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
    }

    public enum SecurityProtocol
    {
        None,
        SSL,
        TLS,
        StartTLS
    }
}
