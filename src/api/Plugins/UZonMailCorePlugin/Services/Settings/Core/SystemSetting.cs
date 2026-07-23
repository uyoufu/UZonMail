using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 系统级设置
    /// </summary>
    public class SystemSetting(string key) : JsonSettings
    {
        protected override async Task<AppSetting?> FetchAppSetting(SqlContext sqlContext)
        {
            return await sqlContext
                .AppSettings.Where(x => x.Key == key)
                .Where(x => x.Type == AppSettingType.System)
                .FirstOrDefaultAsync();
        }
    }
}
