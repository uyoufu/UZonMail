using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Settings;
using UZonMail.DB.SQL.Emails;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    /// <summary>
    /// 代理控制器
    /// </summary>
    public class ProxyController(SqlContext db, ProxyService proxyService, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 验证代理名称是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        [HttpGet("valid-name")]
        public async Task<ResponseResult<bool>> ValidateProxyName(string name)
        {
            var isExist = await proxyService.ValidateProxyName(name);
            if (isExist)
            {
                return ResponseResult<bool>.Fail(isExist.Message);
            }
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="userProxy"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<OrganizationProxy>> CreateProxy(OrganizationProxy userProxy)
        {
            var isExist = await proxyService.ValidateProxyName(userProxy.Name);
            if (isExist)
            {
                return ResponseResult<OrganizationProxy>.Fail(isExist.Message);
            }

            // 验证代理设置是否合法
            if (!ProxyInfo.CanParse(userProxy.Proxy))
            {
                return ResponseResult<OrganizationProxy>.Fail("代理格式不正确");
            }

            var proxy = await proxyService.CreateOrganizationProxy(userProxy);
            return proxy.ToSuccessResponse();
        }

        /// <summary>
        /// 更新代理
        /// </summary>
        /// <param name="userProxy"></param>
        /// <returns></returns>
        [HttpPut()]
        public async Task<ResponseResult<bool>> UpdateProxy(OrganizationProxy userProxy)
        {
            var result = await proxyService.UpdateOrganizationProxy(userProxy);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 删除代理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public async Task<ResponseResult<bool>> DeleteById(long id)
        {
            var organizationId = tokenService.GetOrganizationId();
            db.OrganizationProxies.Remove(new OrganizationProxy { Id = id, OrganizationId = organizationId });
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetFilteredCount(string filter)
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = db.OrganizationProxies.Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Proxy.Contains(filter));
            }

            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取过滤后的数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<OrganizationProxy>>> GetFilteredData(string filter, [FromBody] Pagination pagination)
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = db.OrganizationProxies.Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Proxy.Contains(filter));
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取所有可用的代理
        /// </summary>
        /// <returns></returns>
        [HttpGet("usable")]
        public async Task<ResponseResult<List<OrganizationProxy>>> GetAllUsableProxies()
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = db.OrganizationProxies.Where(x => x.OrganizationId == organizationId).Where(x => x.IsActive);
            var results = await dbSet.ToListAsync();
            return results.ToSuccessResponse();
        }
    }
}
