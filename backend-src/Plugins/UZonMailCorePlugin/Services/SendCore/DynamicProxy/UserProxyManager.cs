using log4net;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.Getters;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy
{
    /// <summary>
    /// 用户代理管理器
    /// </summary>
    public class UserProxyManager(long userId)
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserProxyManager));
        private readonly ConcurrentDictionary<long, IProxyHandler> _proxyHandlers = [];

        public async Task Init(IServiceProvider serviceProvider)
        {
            await UpdateProxies(serviceProvider);
        }

        public async Task UpdateProxies(IServiceProvider serviceProvider)
        {
            var sqlContext = serviceProvider.GetRequiredService<SqlContext>();

            // 获取用户所有的代理
            var proxyGetter = new UserProxyGetter(sqlContext, userId);
            var proxies = await proxyGetter.GetUserProxies();

            var proxyFactories = serviceProvider.GetServices<IProxyFactory>()
                .OrderBy(x => x.Order)
                .ToList();

            // 移除已经不存在的代理
            _proxyHandlers.Keys.Except(proxies.Select(x => x.Id))
                .ToList()
                .ForEach(x => _proxyHandlers.TryRemove(x, out _));

            // 更新或新增代理
            foreach (var proxy in proxies)
            {
                // 若转换器已经存在，则更新
                if (_proxyHandlers.TryGetValue(proxy.Id, out var existOne))
                {
                    existOne.Update(proxy);
                    continue;
                }

                // 若不存在，则新增                
                var newHandler = CreateProxyHandler(proxyFactories, proxy);
                if (newHandler == null) continue;

                _proxyHandlers.TryAdd(newHandler.Id, newHandler);
            }
        }

        /// <summary>
        /// 创建代理处理器
        /// </summary>
        /// <param name="proxyFactories"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        private static IProxyHandler? CreateProxyHandler(List<IProxyFactory> proxyFactories, Proxy proxy)
        {
            foreach (var factory in proxyFactories)
            {
                var handler = factory.CreateProxy(proxy);
                if (handler != null)
                {
                    return handler;
                }
            }

            return null;
        }

        /// <summary>
        /// 随机一个可用代理
        /// </summary>
        /// <param name="ranges">指定代理的范围</param>
        /// <param name="matchStr"></param>
        /// <param name="limitCount">限制每个matchStr对应的使用次数</param>
        /// <returns></returns>
        public IProxyHandler? RandomProxyHandler(List<long> ranges, string matchStr, int limitCount)
        {
            if(ranges.Count==0)return null;
            if (_proxyHandlers.IsEmpty) return null;

            // 若 limitCount 小于等于 0，则不限制
            if (limitCount <= 0) limitCount = int.MaxValue;

            var enabledProxies = _proxyHandlers.Values
                .Where(x=>ranges.Contains(x.Id))
                .Where(x => x.IsEnable())
                .Where(x => x.IsMatch(matchStr, limitCount))
                .ToList();

            // 随机一个代理
            if (enabledProxies.Count == 0)
            {
                _logger.Debug($"未能为 {matchStr} 匹配到代理");
                return null;
            }

            // 返回一个随机代理
            var randomIndex = new Random().Next(0, enabledProxies.Count);
            return enabledProxies[randomIndex];
        }

        /// <summary>
        /// 获取特定的代理处理器
        /// </summary>
        /// <param name="proxyId"></param>
        /// <returns></returns>
        public IProxyHandler? GetProxyHandler(long proxyId)
        {
            _proxyHandlers.TryGetValue(proxyId, out var handler);
            return handler;
        }
    }
}
