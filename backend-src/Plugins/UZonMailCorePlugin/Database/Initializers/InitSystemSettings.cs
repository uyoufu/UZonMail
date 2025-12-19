using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.CorePlugin.Database.Initializers
{
    public class InitSystemSettings(SqlContext db) : IDbInitializer
    {
        public string Name => nameof(InitSystemSettings);

        public async Task ExecuteAsync()
        {
            // 将最新的数据库版本号写入系统设置，防止在初始化时，调用更新逻辑
            await InitDbVersion();
        }

        private async Task InitDbVersion()
        {
            var settingKey = DbVersionInfo.VersionSettingKey;
            var version = DbVersionInfo.RequiredVersion;

            var versionSetting = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == settingKey);
            if (versionSetting != null)
                return;

            // 初始化版本
            versionSetting = new AppSetting { Key = settingKey, StringValue = version.ToString() };
            db.AppSettings.Add(versionSetting);
            await db.SaveChangesAsync();
        }
    }
}
