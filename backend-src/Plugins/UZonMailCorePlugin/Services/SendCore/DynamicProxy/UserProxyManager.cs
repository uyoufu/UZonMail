using log4net;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Concurrent;
using System.Linq;
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
    public class UserProxyManager(long userId) : IProxyHandlerDisposer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserProxyManager));
        private readonly ConcurrentDictionary<string, IProxyHandler> _proxyHandlers = [];

        public async Task UpdateProxies(IServiceProvider serviceProvider)
        {
            var sqlContext = serviceProvider.GetRequiredService<SqlContext>();

            // 获取用户所有的代理
            var proxyGetter = new UserProxyGetter(sqlContext, userId);
            var proxies = await proxyGetter.GetUserProxies();

            // 移除已经不存在的代理
            // 移除不可用的代理集
            _proxyHandlers.Keys.Except(proxies.Select(x => x.ObjectId))
                .ToList()
                .ForEach(x => _proxyHandlers.TryRemove(x, out _));

            // 更新或新增代理
            var proxyFactories = serviceProvider.GetServices<IProxyFactory>()
               .OrderBy(x => x.Order)
               .ToList();

            foreach (var proxy in proxies)
            {
                // 若转换器已经存在，则更新
                if (_proxyHandlers.TryGetValue(proxy.ObjectId, out var existOne))
                {
                    existOne.Update(proxy);
                    continue;
                }

                // 若不存在，则新增                
                var newHandler = await CreateProxyHandler(serviceProvider, proxyFactories, proxy);
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
        private static async Task<IProxyHandler?> CreateProxyHandler(IServiceProvider serviceProvider, List<IProxyFactory> proxyFactories, Proxy proxy)
        {
            foreach (var factory in proxyFactories)
            {
                var handler = await factory.CreateProxy(serviceProvider, proxy);
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
        /// <param name="matchStr"></param>
        /// <param name="limitCount">限制每个matchStr对应的使用次数</param>
        /// <param name="ranges">指定代理的范围, 为空时，默认所有</param>
        /// <returns></returns>
        public IProxyHandler? RandomProxyHandler(string matchStr, int limitCount, List<long>? ranges = null)
        {
            if (_proxyHandlers.IsEmpty) return null;

            // 若 limitCount 小于等于 0，则不限制
            if (limitCount <= 0) limitCount = int.MaxValue;

            var enabledProxies = _proxyHandlers.Values.AsEnumerable();
            if (ranges != null)
            {
                var strRanges = ranges.Select(x => x.ToString()).ToList();
                enabledProxies = enabledProxies.Where(x => strRanges.Contains(x.Id));
            }
            var rangedProxies = enabledProxies
            .Where(x => x.IsEnable())
            .Where(x => x.IsMatch(matchStr, limitCount))
            .ToList();

            // 随机一个代理
            if (rangedProxies.Count == 0)
            {
                _logger.Debug($"未能为 {matchStr} 匹配到代理");
                return null;
            }

            // 返回一个随机代理
            var randomIndex = new Random().Next(0, rangedProxies.Count);
            return rangedProxies[randomIndex];
        }

        /// <summary>
        /// 获取特定的代理处理器
        /// </summary>
        /// <param name="proxyId"></param>
        /// <returns></returns>
        public IProxyHandler? GetProxyHandler(long proxyId)
        {
            _proxyHandlers.TryGetValue(proxyId.ToString(), out var handler);
            return handler;
        }

        public void DisposeHandler()
        {
            foreach (var handler in _proxyHandlers.Values)
            {
                handler.DisposeHandler();
            }
        }
    }
}
