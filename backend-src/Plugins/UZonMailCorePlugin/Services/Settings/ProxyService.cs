using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Settings;
using UZonMail.Core.Utils.Database;

namespace UZonMail.Core.Services.Settings
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
            if (string.IsNullOrEmpty(name)) return StringResult.Fail("代理名称不能为空");

            var organizationId = tokenService.GetOrganizationId();
            bool isExist = await db.OrganizationProxies.AnyAsync(x => x.OrganizationId == organizationId && x.Name == name);
            return new StringResult(isExist, "代理名称已存在");
        }

        /// <summary>
        /// 创建组织代理
        /// </summary>
        /// <param name="organizationProxy"></param>
        /// <returns></returns>
        public async Task<OrganizationProxy> CreateOrganizationProxy(OrganizationProxy organizationProxy)
        {
            var organizationId = tokenService.GetOrganizationId();
            organizationProxy.OrganizationId = organizationId;
            organizationProxy.IsActive = true;
            db.OrganizationProxies.Add(organizationProxy);
            await db.SaveChangesAsync();
            return organizationProxy;
        }

        /// <summary>
        /// 更新代理
        /// </summary>
        /// <param name="userProxy"></param>
        /// <returns></returns>
        public async Task<bool> UpdateOrganizationProxy(OrganizationProxy userProxy)
        {
            var organizationId = tokenService.GetOrganizationId();
            await db.OrganizationProxies.UpdateAsync(x => x.OrganizationId == organizationId && x.Id == userProxy.Id,
                x => x.SetProperty(y => y.Name, userProxy.Name)
                    .SetProperty(y => y.Description, userProxy.Description)
                    .SetProperty(y => y.Proxy, userProxy.Proxy)
                    .SetProperty(y => y.IsActive, userProxy.IsActive)
                    .SetProperty(y => y.MatchRegex, userProxy.MatchRegex)
                    .SetProperty(y => y.Priority, userProxy.Priority)
                );
            return true;
        }
    }
}
