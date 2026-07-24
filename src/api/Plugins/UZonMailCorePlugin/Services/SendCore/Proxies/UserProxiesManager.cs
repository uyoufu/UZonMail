using System.Collections.Concurrent;
using log4net;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.Getters;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies
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
            if (!Uri.TryCreate(proxy.Url, UriKind.Absolute, out var uri))
            {
                _logger.Error($"代理 {proxy.Id} 的 URL 格式无效");
                return null;
            }

            var matches = proxyFactories.Where(x => x.CanHandle(uri)).ToList();
            if (matches.Count != 1)
            {
                var kinds = string.Join(", ", matches.Select(x => x.Kind));
                _logger.Error(
                    matches.Count == 0
                        ? $"代理 {proxy.Id} 未匹配到代理类型"
                        : $"代理 {proxy.Id} 同时匹配多个代理类型: {kinds}"
                );
                return null;
            }

            return await matches[0].CreateProxy(serviceProvider, proxy);
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
                .OrderByDescending(x => x.Priority)
                .ToList();

            if (rangedProxies.Count == 0)
            {
                _logger.Debug($"未能为 {matchStr} 匹配到代理");
                return null;
            }

            var highestPriority = rangedProxies[0].Priority;
            var preferred = rangedProxies.TakeWhile(x => x.Priority == highestPriority).ToList();
            return preferred[Random.Shared.Next(0, preferred.Count)];
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
