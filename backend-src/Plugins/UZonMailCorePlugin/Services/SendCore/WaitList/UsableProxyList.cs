using System.Collections.Concurrent;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.DB.SQL.Settings;

namespace UZonMail.Core.Services.SendCore.EmailWaitList
{
    public class UsableProxyList(long userId)
    {
        private readonly ConcurrentDictionary<long, long> _sendingItemProxies = [];
        private readonly CacheManager _cacheManager = new();

        /// <summary>
        /// 添加发送项代理
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <param name="proxyId"></param>
        public void AddSendingItemProxy(long sendingItemId, long proxyId)
        {
            if (proxyId <= 0) return;
            _sendingItemProxies.TryAdd(sendingItemId, proxyId);
        }

        /// <summary>
        /// 为发件项获取代理
        /// 当发件项未被指定代理时，从用户的代理中获取一个匹配的代理
        /// 当发件项被指定代理时，返回指定的代理
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sendingItemId">发件项id，用于匹配代理</param>
        /// <param name="outboxEmail">发件箱，用于匹配代理</param>
        /// <returns></returns>
        public async Task<ProxyInfo?> GetProxy(SqlContext db,long sendingItemId,string outboxEmail)
        {
            // 获取所有的代理
            var allProxies = await _cacheManager.GetCache<UserProxiesCache>(db, userId);
            if (allProxies.Count == 0) return null;

            Proxy? proxy = null;
            if (_sendingItemProxies.TryGetValue(sendingItemId, out var proxyId))
            {
                // 从所有的代理中查找
                proxy = allProxies.Where(x=>x.Id == proxyId).FirstOrDefault(); 
            }
            else
            {
                // 没有指定代理时，随机获取一个代理
                proxy = allProxies.Where(x => x.IsMatch(outboxEmail)).FirstOrDefault();
            }

            if (proxy == null)
            {
                // 说明没有可用的代理
                return null;
            }

            return new ProxyInfo(proxy.Url);
        }
    }
}
