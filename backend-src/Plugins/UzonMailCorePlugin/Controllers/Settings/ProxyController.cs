using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Settings
{
    /// <summary>
    /// 代理控制器
    /// </summary>
    public class ProxyController(
        SqlContext db,
        ProxyService proxyService,
        TokenService tokenService
    ) : ControllerBaseV1
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
        public async Task<ResponseResult<Proxy>> CreateProxy(Proxy userProxy)
        {
            var isExist = await proxyService.ValidateProxyName(userProxy.Name);
            if (isExist)
            {
                return ResponseResult<Proxy>.Fail(isExist.Message);
            }

            // 验证代理设置是否合法
            if (!ProxyInfo.CanParse(userProxy.Url))
            {
                return ResponseResult<Proxy>.Fail("代理格式不正确");
            }

            var proxy = await proxyService.CreateProxy(userProxy);
            return proxy.ToSuccessResponse();
        }

        /// <summary>
        /// 更新代理
        /// </summary>
        /// <param name="userProxy"></param>
        /// <returns></returns>
        [HttpPut()]
        public async Task<ResponseResult<bool>> UpdateProxy(Proxy userProxy)
        {
            var result = await proxyService.UpdateProxy(userProxy);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 修改代理的共享状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShared"></param>
        /// <returns></returns>
        [HttpPut("{id:long}/shared")]
        public async Task<ResponseResult<bool>> UpdateProxyShareStatus(long id, bool isShared)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var organizationId = isShared ? tokenPayloads.OrganizationId : 0;

            await db.Proxies.UpdateAsync(
                x => x.Id == id && x.UserId == tokenPayloads.UserId,
                x =>
                    x.SetProperty(y => y.OrganizationId, organizationId)
                        .SetProperty(y => y.IsShared, isShared)
            );
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 删除代理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public async Task<ResponseResult<bool>> DeleteById(long id)
        {
            // 只能删除自己的代理
            var userId = tokenService.GetUserSqlId();
            db.Proxies.Remove(new Proxy { Id = id, UserId = userId });
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取过滤后的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetFilteredCount(string filter)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db.Proxies.Where(x =>
                (x.OrganizationId == tokenPayloads.OrganizationId && x.IsShared)
                || x.UserId == tokenPayloads.UserId
            );
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Url.Contains(filter));
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
        public async Task<ResponseResult<List<Proxy>>> GetFilteredData(
            string filter,
            [FromBody] Pagination pagination
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .Proxies.Where(x => x.IsActive)
                .Where(x =>
                    (x.OrganizationId == tokenPayloads.OrganizationId && x.IsShared)
                    || x.UserId == tokenPayloads.UserId
                );
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Url.Contains(filter));
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取过滤后的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("enabled/filtered-count")]
        public async Task<ResponseResult<int>> GetEnabledFilteredCount(string filter)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .Proxies.Where(x =>
                    (x.OrganizationId == tokenPayloads.OrganizationId && x.IsShared)
                    || x.UserId == tokenPayloads.UserId
                )
                .Where(x => x.IsActive);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Url.Contains(filter));
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
        [HttpPost("enabled/filtered-data")]
        public async Task<ResponseResult<List<Proxy>>> GetEnabledFilteredData(
            string filter,
            [FromBody] Pagination pagination
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .Proxies.Where(x => x.IsActive)
                .Where(x =>
                    (x.OrganizationId == tokenPayloads.OrganizationId && x.IsShared)
                    || x.UserId == tokenPayloads.UserId
                )
                .Where(x => x.IsActive);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Url.Contains(filter));
            }
            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取所有可用的代理
        /// </summary>
        /// <returns></returns>
        [HttpGet("usable")]
        public async Task<ResponseResult<List<Proxy>>> GetAllUsableProxies()
        {
            var organizationId = tokenService.GetOrganizationId();
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .Proxies.Where(x => x.IsActive)
                .Where(x =>
                    x.OrganizationId == tokenPayloads.OrganizationId
                    || x.UserId == tokenPayloads.UserId
                );

            var results = await dbSet.ToListAsync();
            return results.ToSuccessResponse();
        }
    }
}
