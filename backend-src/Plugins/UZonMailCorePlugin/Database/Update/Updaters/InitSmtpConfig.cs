using UZonMail.Core.Database.Updater;

namespace UZonMail.Core.Database.Update.Updaters
{
    public class InitSmtpConfig : IDatabaseUpdater
    {
        public Version Version => new("0.12.4");

        public async Task Update()
        {
            // 从当前目录下的配置中读取SMTP配置
            var smtpConfigPath = Path.Combine(AppContext.BaseDirectory, "smtp.json");
        }
    }
}
