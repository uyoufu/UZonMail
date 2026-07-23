using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using log4net;
using UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    /// <summary>
    /// 动态代理池基类。
    /// </summary>
    public abstract class ProxyHandlersCluster : ProxyHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandlersCluster));
        private readonly ConcurrentDictionary<string, ProxyHandler> _handlers = [];
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private readonly int _minimumCount = 1;
        private volatile bool _isHealthless;
        private volatile bool _disposed;

        protected abstract Task<List<ProxyHandler>> GetProxyHandlersAsync(
            IServiceProvider serviceProvider
        );

        public override async Task<ProxyClientAdapter?> GetProxyClientAsync(
            IServiceProvider scopeServiceProvider,
            string email
        )
        {
            if (_disposed)
                return null;

            CleanupExpiredResources();

            if (_handlers.Count < _minimumCount)
                await UpdateProxyHandlers(scopeServiceProvider);

            var matchedHandler = await SelectMatchedHandler(scopeServiceProvider, email);
            if (matchedHandler == null)
            {
                await UpdateProxyHandlers(scopeServiceProvider);
                matchedHandler = await SelectMatchedHandler(scopeServiceProvider, email);
            }

            if (matchedHandler == null)
            {
                _logger.Warn($"没有可用的代理客户端匹配 {email}");
                return null;
            }

            return await matchedHandler.GetProxyClientAsync(scopeServiceProvider, email);
        }

        private async Task<ProxyHandler?> SelectMatchedHandler(
            IServiceProvider scopeServiceProvider,
            string email
        )
        {
            if (_handlers.IsEmpty)
                return null;

            var domain = GetEmailDomain(email);
            var ipRateLimiter = scopeServiceProvider.GetRequiredService<IPRateLimiter>();
            var settingsManager = scopeServiceProvider.GetRequiredService<AppSettingsManager>();
            var sqlContext = scopeServiceProvider.GetRequiredService<SqlContext>();
            var sendingSetting = await settingsManager.GetSetting<SendingSetting>(sqlContext, UserId);

            return _handlers
                .Values.Where(x => x.IsMatch(email) && x.IsEnable())
                .Where(x =>
                    !ipRateLimiter.IsLimited(
                        domain,
                        x.Host,
                        sendingSetting.MaxCountPerIPDomainHour
                    )
                )
                .FirstOrDefault();
        }

        private async Task UpdateProxyHandlers(IServiceProvider serviceProvider)
        {
            await _refreshLock.WaitAsync();
            try
            {
                if (_disposed)
                    return;

                var handlers = await GetProxyHandlersAsync(serviceProvider);
                foreach (var handler in handlers)
                {
                    if (!await handler.HealthCheck())
                    {
                        handler.DisposeHandler();
                        continue;
                    }

                    if (_handlers.TryGetValue(handler.Id, out var existOne))
                    {
                        if (_handlers.TryUpdate(handler.Id, handler, existOne))
                            existOne.DisposeHandler();
                        else
                            handler.DisposeHandler();
                    }
                    else if (!_handlers.TryAdd(handler.Id, handler))
                    {
                        handler.DisposeHandler();
                    }
                }

                if (_handlers.IsEmpty)
                    MarkHealthless();
                else
                    _isHealthless = false;
            }
            catch (Exception ex)
            {
                MarkHealthless();
                _logger.Warn($"动态代理 {Id} 更新失败");
                _logger.Warn(ex);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public override bool IsMatch(string email)
        {
            if (ProxyInfo == null)
                return false;

            if (string.IsNullOrEmpty(ProxyInfo.MatchRegex))
                return true;

            try
            {
                return Regex.IsMatch(email, ProxyInfo.MatchRegex);
            }
            catch
            {
                return false;
            }
        }

        public override bool IsEnable()
        {
            return !_disposed && !_isHealthless && ProxyInfo?.IsActive != false;
        }

        public override void Update(
            Proxy proxy,
            ProxyZoneType proxyZoneType = ProxyZoneType.Default,
            int expireSeconds = int.MaxValue,
            int maxUsedCountPerDomain = -1,
            long userId = 0
        )
        {
            ProxyInfo = proxy;
            if (userId > 0)
                UserId = userId;

            _disposed = false;
            _isHealthless = false;
        }

        public override void MarkHealthless()
        {
            _isHealthless = true;
        }

        public override bool ShouldHealthCheck => false;

        public override Task<bool> HealthCheck()
        {
            return Task.FromResult(IsEnable());
        }

        protected override void AutoHealthCheck()
        {
            // 动态代理池按需刷新，不参与集中健康检测。
        }

        public override void CleanupExpiredResources()
        {
            foreach (var handler in _handlers.Values.Where(x => !x.IsEnable() || x.IsExpired).ToList())
            {
                if (_handlers.TryRemove(handler.Id, out var removedHandler))
                    removedHandler.DisposeHandler();
            }
        }

        public override void DisposeHandler()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var handler in _handlers.Values)
                handler.DisposeHandler();
            _handlers.Clear();
            base.DisposeHandler();
        }

        protected ProxyHandler CreateProxyHandler(
            IServiceProvider serviceProvider,
            Proxy proxy,
            ProxyZoneType proxyZoneType,
            int expireSeconds
        )
        {
            var handler = serviceProvider.GetRequiredService<ProxyHandler>();
            handler.Update(proxy, proxyZoneType, expireSeconds, userId: UserId);
            return handler;
        }

        protected int GetExpireMinutes(string url)
        {
            var match = Regex.Match(url, "expireMinutes=(\\d+)");
            if (!match.Success && ProxyInfo != null)
                match = Regex.Match(ProxyInfo.Url, "expireMinutes=(\\d+)");

            return match.Success ? int.Parse(match.Groups[1].Value) : 5;
        }

        protected string GetProtocol(string url)
        {
            var match = Regex.Match(url, "protocol=(socks4|socks5|http|https)");
            if (!match.Success && ProxyInfo != null)
                match = Regex.Match(ProxyInfo.Url, "protocol=(socks4|socks5|http|https)");

            return match.Success ? match.Groups[1].Value : "socks5";
        }

        private static string GetEmailDomain(string email)
        {
            var index = email.LastIndexOf('@');
            return index >= 0 && index < email.Length - 1 ? email[(index + 1)..] : email;
        }
    }
}
