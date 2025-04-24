namespace UZonMail.Core.Controllers.Settings.Models
{
    public class SystemSmtpNotificationSetting
    {
        public string Email { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string Password { get; set; }

        public bool IsValid { get; set; } = false;
    }
}
