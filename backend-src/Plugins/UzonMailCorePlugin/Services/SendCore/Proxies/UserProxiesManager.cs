using System.Collections.Concurrent;
using log4net;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.Getters;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.CorePlugin.Services.SendCore.Proxies
{
    /// <summary>
    /// 用户代理管理器。
    /// </summary>
    public class UserProxiesManager(long userId) : IProxyHandlerDisposer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserProxiesManager));
        private readonly ConcurrentDictionary<string, IProxyHandler> _proxyHandlers = [];
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private volatile bool _loaded;

        public DateTime LastAccessDate { get; private set; } = DateTime.UtcNow;

        public int HandlerCount => _proxyHandlers.Count;

        public async Task EnsureLoadedAsync(IServiceProvider serviceProvider)
        {
            if (_loaded)
                return;

            await UpdateProxies(serviceProvider);
        }

        public async Task UpdateProxies(IServiceProvider serviceProvider)
        {
            await _updateLock.WaitAsync();
            try
            {
                Touch();

                var sqlContext = serviceProvider.GetRequiredService<SqlContext>();
                var proxyGetter = new UserProxyGetter(sqlContext, userId);
                var proxies = await proxyGetter.GetUserProxies();

                var activeProxyKeys = proxies.Select(GetProxyKey).ToHashSet();
                foreach (var removedKey in _proxyHandlers.Keys.Except(activeProxyKeys).ToList())
                {
                    if (_proxyHandlers.TryRemove(removedKey, out var removedHandler))
                        removedHandler.DisposeHandler();
                }

                var proxyFactories = serviceProvider
                    .GetServices<IProxyFactory>()
                    .OrderBy(x => x.Order)
                    .ToList();

                var settingManager = serviceProvider.GetRequiredService<AppSettingsManager>();
                var sendingSetting = await settingManager.GetSetting<SendingSetting>(
                    sqlContext,
                    userId
                );

                foreach (var proxy in proxies.OrderByDescending(x => x.Priority))
                {
                    var proxyKey = GetProxyKey(proxy);
                    if (!_proxyHandlers.TryGetValue(proxyKey, out var existOne))
                    {
                        var newHandler = await CreateProxyHandler(
                            serviceProvider,
                            proxyFactories,
                            proxy
                        );
                        if (newHandler == null)
                            continue;

                        newHandler.Update(
                            proxy,
                            maxUsedCountPerDomain: sendingSetting.ChangeIpAfterEmailCount,
                            userId: userId
                        );

                        if (!_proxyHandlers.TryAdd(newHandler.Id, newHandler))
                        {
                            newHandler.DisposeHandler();
                            continue;
                        }

                        existOne = newHandler;
                    }
                    else
                    {
                        existOne.Update(
                            proxy,
                            maxUsedCountPerDomain: sendingSetting.ChangeIpAfterEmailCount,
                            userId: userId
                        );
                    }

                    if (existOne is IProxyHealthCheckable healthCheckable)
                        await healthCheckable.HealthCheck();
                }

                _loaded = true;
            }
            finally
            {
                _updateLock.Release();
            }
        }

        private static async Task<IProxyHandler?> CreateProxyHandler(
            IServiceProvider serviceProvider,
            List<IProxyFactory> proxyFactories,
            Proxy proxy
        )
        {
            foreach (var factory in proxyFactories)
            {
                var handler = await factory.CreateProxy(serviceProvider, proxy);
                if (handler != null)
                    return handler;
            }

            return null;
        }

        public IProxyHandler? RandomProxyHandler(string matchStr, List<long>? ranges = null)
        {
            Touch();

            if (_proxyHandlers.IsEmpty)
                return null;

            var enabledProxies = _proxyHandlers.Values.AsEnumerable();
            if (ranges is { Count: > 0 })
            {
                var strRanges = ranges.Select(x => x.ToString()).ToHashSet();
                enabledProxies = enabledProxies.Where(x => strRanges.Contains(x.Id));
            }

            var rangedProxies = enabledProxies
                .Where(x => x.IsEnable())
                .Where(x => x.IsMatch(matchStr))
                .ToList();

            if (rangedProxies.Count == 0)
            {
                _logger.Debug($"未能为 {matchStr} 匹配到代理");
                return null;
            }

            var randomIndex = Random.Shared.Next(0, rangedProxies.Count);
            return rangedProxies[randomIndex];
        }

        public IProxyHandler? GetProxyHandler(long proxyId)
        {
            Touch();
            _proxyHandlers.TryGetValue(proxyId.ToString(), out var handler);
            return handler;
        }

        public async Task MaintainAsync()
        {
            foreach (var handler in _proxyHandlers.Values.OfType<IProxyResourceCleaner>())
                handler.CleanupExpiredResources();

            var healthCheckHandlers = _proxyHandlers
                .Values.OfType<IProxyHealthCheckable>()
                .Where(x => x.ShouldHealthCheck)
                .ToList();

            await Parallel.ForEachAsync(
                healthCheckHandlers,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (handler, _) => await handler.HealthCheck()
            );
        }

        public void DisposeHandler()
        {
            foreach (var handler in _proxyHandlers.Values)
                handler.DisposeHandler();

            _proxyHandlers.Clear();
            _loaded = false;
        }

        private void Touch()
        {
            LastAccessDate = DateTime.UtcNow;
        }

        private static string GetProxyKey(Proxy proxy)
        {
            return proxy.Id > 0 ? proxy.Id.ToString() : proxy.ObjectId;
        }
    }
}
