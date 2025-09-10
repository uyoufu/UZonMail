using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Proxies.Clients;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.Proxies
{
    /// <summary>
    /// 代理管理器
    /// </summary>
    public class ProxiesManager : ISingletonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxiesManager));
        private readonly AppSettingsManager _settingsService;

        public ProxiesManager(AppSettingsManager settingsService)
        {
            _settingsService = settingsService;

            DisposeHandlerAuto();
        }

        private readonly ConcurrentDictionary<long, UserProxiesManager> _userProxyManagers = new();

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

            return await GetProxyHandler(sendingContext.Provider, userId, outboxEmail, sendingContext.EmailItem.ProxyId, sendingContext.EmailItem.AvailableProxyIds);
        }

        /// <summary>
        /// 返回指定代理
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="userId"></param>
        /// <param name="outboxEmail"></param>
        /// <param name="proxyId"></param>
        /// <returns></returns>
        public async Task<IProxyHandler?> GetProxyHandler(IServiceProvider serviceProvider, long userId, string outboxEmail, long proxyId, List<long> availableProxyIds = null)
        {
            if (!_userProxyManagers.TryGetValue(userId, out var manager))
            {
                // 新增并添加
                manager = new UserProxiesManager(userId);
                await manager.UpdateProxies(serviceProvider);
                _userProxyManagers.TryAdd(userId, manager);
            }

            if (proxyId > 0)
            {
                // 返回特定的代理
                return manager.GetProxyHandler(proxyId);
            }
            if (availableProxyIds == null || availableProxyIds.Count == 0)
            {
                return null;
            }

            // 随机匹配代理
            return manager.RandomProxyHandler(outboxEmail, availableProxyIds);
        }

        /// <summary>
        /// 随机匹配用户下的代理
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="userId"></param>
        /// <param name="matchStr"></param>
        /// <returns></returns>
        public async Task<IProxyHandler?> GetProxyHander(IServiceProvider serviceProvider, long userId, string matchStr)
        {
            if (!_userProxyManagers.TryGetValue(userId, out var manager))
            {
                // 新增并添加
                manager = new UserProxiesManager(userId);
                await manager.UpdateProxies(serviceProvider);
                _userProxyManagers.TryAdd(userId, manager);
            }

            // 随机匹配代理
            return manager.RandomProxyHandler(matchStr);
        }


        private Timer? _timer;
        /// <summary>
        /// 释放资源
        /// </summary>
        private void DisposeHandlerAuto()
        {
            if (_timer != null) return;

            _timer = new Timer(_ =>
            {
                _logger.Info("开始自动释放代理处理器");
                foreach (var manager in _userProxyManagers.Values)
                {
                    manager.DisposeHandler();
                }
            }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
    }
}
