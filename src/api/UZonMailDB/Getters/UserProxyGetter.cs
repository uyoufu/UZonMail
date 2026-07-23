using Microsoft.EntityFrameworkCore;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.DB.Getters
{
    public class UserProxyGetter(SqlContext db, long userId)
    {
        /// <summary>
        /// 获取用户可获取的所有代理
        /// </summary>
        /// <returns></returns>
        public async Task<List<Proxy>> GetUserProxies()
        {
            var userInfo = await DBCacheManager.Global.GetCache<UserInfoCache>(db, userId);
            // 按用户缓存代理
            var results = await db
                .Proxies.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Where(x => x.IsActive) // 必须是激活状态
                .Where(x =>
                    x.UserId == userId
                    || (x.IsShared && x.OrganizationId == userInfo.OrganizationId)
                )
                .ToListAsync();
            return results;
        }
    }
}
