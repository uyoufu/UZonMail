using System.Collections.Concurrent;
using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Proxies
{
    /// <summary>
    /// 代理管理器。
    /// </summary>
    public class ProxiesManager : ISingletonService, IAsyncDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxiesManager));
        private static readonly TimeSpan MaintainInterval = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan EmptyManagerExpireTime = TimeSpan.FromMinutes(30);

        private readonly ConcurrentDictionary<long, UserProxiesManager> _userProxyManagers = new();
        private readonly CancellationTokenSource _maintainCts = new();
        private readonly Task _maintainTask;

        public ProxiesManager()
        {
            _maintainTask = MaintainLoopAsync(_maintainCts.Token);
        }

        public async Task UpdateUserProxies(IServiceProvider serviceProvider, long userId)
        {
            var manager = _userProxyManagers.GetOrAdd(userId, static id => new UserProxiesManager(id));
            await manager.UpdateProxies(serviceProvider);
        }

        public async Task<IProxyHandler?> GetProxyHandler(SendingContext sendingContext)
        {
            if (sendingContext.EmailItem == null)
                return null;

            var userId = sendingContext.EmailItem.UserId;
            var outboxEmail = sendingContext.EmailItem.Outbox.Email;

            return await GetProxyHandler(
                sendingContext.Provider,
                userId,
                outboxEmail,
                sendingContext.EmailItem.ProxyId,
                sendingContext.EmailItem.AvailableProxyIds
            );
        }

        public async Task<IProxyHandler?> GetProxyHandler(
            IServiceProvider serviceProvider,
            long userId,
            string outboxEmail,
            long proxyId,
            List<long>? availableProxyIds = null
        )
        {
            var manager = _userProxyManagers.GetOrAdd(userId, static id => new UserProxiesManager(id));
            await manager.EnsureLoadedAsync(serviceProvider);

            if (proxyId > 0)
                return manager.GetProxyHandler(proxyId);

            if (availableProxyIds == null || availableProxyIds.Count == 0)
                return null;

            return manager.RandomProxyHandler(outboxEmail, availableProxyIds);
        }

        public async Task<IProxyHandler?> GetProxyHander(
            IServiceProvider serviceProvider,
            long userId,
            string matchStr
        )
        {
            var manager = _userProxyManagers.GetOrAdd(userId, static id => new UserProxiesManager(id));
            await manager.EnsureLoadedAsync(serviceProvider);

            return manager.RandomProxyHandler(matchStr);
        }

        private async Task MaintainLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var timer = new PeriodicTimer(MaintainInterval);
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    await MaintainManagersAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // 正常释放。
            }
            catch (Exception ex)
            {
                _logger.Warn("代理维护循环异常退出");
                _logger.Warn(ex);
            }
        }

        private async Task MaintainManagersAsync()
        {
            foreach (var pair in _userProxyManagers.ToList())
            {
                var manager = pair.Value;
                await manager.MaintainAsync();

                if (
                    manager.HandlerCount == 0
                    && DateTime.UtcNow - manager.LastAccessDate > EmptyManagerExpireTime
                    && _userProxyManagers.TryRemove(pair.Key, out var removedManager)
                )
                {
                    removedManager.DisposeHandler();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _maintainCts.Cancel();
            try
            {
                await _maintainTask;
            }
            catch (OperationCanceledException)
            {
                // 正常释放。
            }
            _maintainCts.Dispose();

            foreach (var manager in _userProxyManagers.Values)
                manager.DisposeHandler();
            _userProxyManagers.Clear();
        }
    }
}
