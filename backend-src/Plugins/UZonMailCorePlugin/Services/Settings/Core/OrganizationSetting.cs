using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.DB.SQL;
using Microsoft.EntityFrameworkCore;

namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 组织级设置
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="data"></param>
    public class OrganizationSetting(string key, long organizationId) : HierarchicalSetting(AppSettingType.Organization, key, organizationId)
    {
        /// <summary>
        /// 拉取设置
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <returns></returns>
        protected override async Task<AppSetting?> FetchAppSetting(SqlContext sqlContext)
        {
            return await sqlContext.AppSettings
                  .Where(x => x.Key == Key)
                  .Where(x => x.Type == AppSettingType.Organization && x.OrganizationId == OwnerId)
                  .FirstOrDefaultAsync();
        }
    }
}
