using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.DB.SQL;
using Microsoft.EntityFrameworkCore;

namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 系统级设置
    /// </summary>
    public class SystemSetting(string key) : HierarchicalSetting(AppSettingType.System, key, 0)
    {
        protected override async Task<AppSetting?> FetchAppSetting(SqlContext sqlContext)
        {
            return await sqlContext.AppSettings
                  .Where(x => x.Key == Key)
                  .Where(x => x.Type == AppSettingType.System)
                  .FirstOrDefaultAsync();
        }
    }
}
