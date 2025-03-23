using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.DynamicProxy
{
    /// <summary>
    /// 代理管理器
    /// </summary>
    public class ProxyManager : ISingletonService
    {
        private readonly ConcurrentDictionary<long, UserProxyManager> _userProxyManagers = new();

        /// <summary>
        /// 重新加载用户的代理
        /// </summary>
        /// <returns></returns>
        public async Task UpdateUserProxies(IServiceProvider serviceProvider, long userId)
        {
            if (_userProxyManagers.TryGetValue(userId, out var value))
            {
                await value.UpdateProxies(serviceProvider);
            }
        }


        /// <summary>
        /// 获取代理处理器
        /// </summary>
        /// <param name="userId">用户ID</param></param>
        /// <param name="matchStr">匹配字符，比如发件箱号</param>
        /// <returns></returns>
        public async Task<IProxyHandler?> GetProxyHandler(SendingContext sendingContext)
        {
            if (sendingContext.EmailItem == null) return null;

            var userId = sendingContext.EmailItem.UserId;
            var outboxEmail = sendingContext.EmailItem.Outbox.Email;

            if (!_userProxyManagers.TryGetValue(userId, out var manager))
            {
                // 新增并添加
                manager = new UserProxyManager(userId);
                await manager.UpdateProxies(sendingContext.Provider);
                _userProxyManagers.TryAdd(userId, manager);
            }

            if (sendingContext.EmailItem.ProxyId > 0)
            {
                // 返回特定的代理
                return manager.GetProxyHandler(sendingContext.EmailItem.ProxyId);
            }

            var orgSetting = await CacheManager.Global.GetCache<OrganizationSettingCache>(sendingContext.SqlContext, userId);
            // 随机匹配代理
            return manager.RandomProxyHandler(sendingContext.EmailItem.AvailableProxyIds, outboxEmail, orgSetting.ChangeIpAfterEmailCount);
        }
    }
}
