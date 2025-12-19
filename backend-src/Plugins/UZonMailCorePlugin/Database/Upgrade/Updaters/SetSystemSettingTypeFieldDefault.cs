using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.CorePlugin.Database.Upgrade.Updaters
{
    /// <summary>
    /// 为既有系统设置的类型添加默认值
    /// </summary>
    /// <param name="db"></param>
    public class SetSystemSettingTypeFieldDefault(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.13.0");

        public async Task ExecuteAsync()
        {
            await db.AppSettings.UpdateAsync(
                x => x.Type == AppSettingType.None,
                x => x.SetProperty(x => x.Type, AppSettingType.System)
            );
        }
    }
}
