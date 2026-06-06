using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Utils.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.Settings.Core
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
