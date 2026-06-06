using MailKit.Security;
using UZonMail.DB.SQL.Core.Emails;

namespace UZonMail.CorePlugin.Services.Settings.Model
{
    /// <summary>
    /// smtp 通知设置
    /// 若 isValid 为 false，则表示未设置该项, 需要忽略当前设置
    /// </summary>
    public class SmtpNotificationSetting : BaseSettingModel
    {
        public string Email { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        // 该密码是明文存储的
        public string Password { get; set; }

        // 安全套接字选项
        public ConnectionSecurity ConnectionSecurity { get; set; } = ConnectionSecurity.SSL;

        /// <summary>
        /// 是否验证有效
        /// </summary>
        public bool IsValid { get; set; }

        protected override void ReadValuesFromJsons()
        {
            Email = GetStringValue(nameof(Email));
            SmtpHost = GetStringValue(nameof(SmtpHost));
            SmtpPort = GetIntValue(nameof(SmtpPort));
            Password = GetStringValue(nameof(Password));
            IsValid = GetBoolValue(nameof(IsValid), false);
        }
    }
}
