using UZonMail.Core.Database.Updater;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Database.Update.Updaters
{
    /// <summary>
    /// 为既有系统设置的类型添加默认值
    /// </summary>
    /// <param name="db"></param>
    public class SetSystemSettingTypeFieldDefault(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.13.0");

        public async Task Update()
        {
            await db.AppSettings.UpdateAsync(x => x.Type == null, x => x.SetProperty(x => x.Type, AppSettingType.System));
        }
    }
}
