using Microsoft.EntityFrameworkCore;
using UzonMail.Core.Utils.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 用户设置
    /// </summary>
    /// <param name="type"></param>
    /// <param name="userId"></param>
    /// <param name="data"></param>
    public class UserSetting(CacheKey cacheKey) : JsonSettings
    {
        protected override async Task<AppSetting?> FetchAppSetting(SqlContext sqlContext)
        {
            return await sqlContext
                .AppSettings.Where(x => x.Key == cacheKey.Key)
                .Where(x => x.Type == AppSettingType.User && x.UserId == cacheKey.OwnerId)
                .FirstOrDefaultAsync();
        }
    }
}
