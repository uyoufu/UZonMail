namespace UZonMail.Core.Services.Settings.Model
{
    public class SmtpNotificationSetting : BaseSettingModel
    {
        public string Email { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string Password { get; set; }

        public bool IsValid { get; set; } = false;

        protected override void InitValue()
        {
            var setting = HierarchicalSetting.ToObjects<SmtpNotificationSetting>()
                .Where(x => x.IsValid)
                .FirstOrDefault();
            if (setting == null) return;

            Email = setting.Email;
            SmtpHost = setting.SmtpHost;
            SmtpPort = setting.SmtpPort;
            Password = setting.Password;
            IsValid = setting.IsValid;
        }
    }
}
