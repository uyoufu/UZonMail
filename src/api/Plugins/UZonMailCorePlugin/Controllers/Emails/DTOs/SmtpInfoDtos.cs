using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Controllers.Emails.DTOs
{
    /// <summary>
    /// 更新 SMTP 推断信息时允许客户端提交的数据。
    /// </summary>
    public class UpdateSmtpInfoDto
    {
        public string Domain { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public ConnectionSecurity ConnectionSecurity { get; set; }
        public bool EnableSSL { get; set; }
    }
}
