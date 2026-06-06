using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Utils.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 组织级设置
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="data"></param>
    public class OrganizationSetting(CacheKey key) : JsonSettings(key)
    {
        /// <summary>
        /// 拉取设置
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        protected override async Task<AppSetting?> FetchAppSetting(SqlContext sqlContext)
        {
            return await sqlContext
                .AppSettings.Where(x => x.Key == key.Key)
                .Where(x =>
                    x.Type == AppSettingType.Organization && x.OrganizationId == key.OwnerId
                )
                .FirstOrDefaultAsync();
        }
    }
}
