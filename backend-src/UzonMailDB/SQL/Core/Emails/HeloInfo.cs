using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Emails
{
    /// <summary>
    /// HELO 信息库
    /// 系统使用该库进行 HELO 验证，判断发件箱是否有效
    /// </summary>
    public class HeloInfo : SqlId
    {
        /// <summary>
        /// 主机名
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// 是否启用加密
        /// </summary>
        public bool EnableSSL { get; set; } = false;
    }
}
