using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Utils.Database;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.Settings
{
    /// <summary>
    /// 代理服务
    /// </summary>
    public class ProxyService(SqlContext db, TokenService tokenService) : IScopedService
    {
        /// <summary>
        /// 验证代理名称是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<StringResult> ValidateProxyName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return StringResult.Fail("代理名称不能为空");

            var organizationId = tokenService.GetOrganizationId();
            bool isExist = await db.Proxies.AnyAsync(x =>
                x.OrganizationId == organizationId && x.Name == name
            );
            return new StringResult(isExist, "代理名称已存在");
        }

        /// <summary>
        /// 创建组织代理
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public async Task<Proxy> CreateProxy(Proxy proxy)
        {
            var tokenInfo = tokenService.GetTokenPayloads();
            proxy.IsActive = true;
            proxy.OrganizationId = proxy.IsShared ? tokenInfo.OrganizationId : 0;
            proxy.UserId = tokenInfo.UserId;

            db.Proxies.Add(proxy);
            await db.SaveChangesAsync();
            return proxy;
        }

        /// <summary>
        /// 更新代理
        /// </summary>
        /// <param name="userProxy"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProxy(Proxy userProxy)
        {
            // 只能更新自己的代理
            var userId = tokenService.GetUserSqlId();
            await db.Proxies.UpdateAsync(
                x => x.UserId == userId && x.Id == userProxy.Id,
                x =>
                    x.SetProperty(y => y.Name, userProxy.Name)
                        .SetProperty(y => y.Description, userProxy.Description)
                        .SetProperty(y => y.Url, userProxy.Url)
                        .SetProperty(y => y.IsActive, userProxy.IsActive)
                        .SetProperty(y => y.MatchRegex, userProxy.MatchRegex)
                        .SetProperty(y => y.Priority, userProxy.Priority)
            );
            return true;
        }
    }
}
