using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using log4net;
using UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    /// <summary>
    /// 处理单个代理端点。
    /// </summary>
    public class ProxyHandler
        : IProxyHandler,
            IProxyHealthCheckable,
            IProxyResourceCleaner
    {
        protected ProxyHandler()
        {
            _iPQueries = [];
        }

        public ProxyHandler(IEnumerable<IProxyHealthChecker> iPQueries)
        {
            _iPQueries = [.. iPQueries];
        }

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandler));
        private static readonly TimeSpan HealthCheckInterval = TimeSpan.FromSeconds(20);

        private readonly List<IProxyHealthChecker> _iPQueries;
        private readonly SemaphoreSlim _healthCheckLock = new(1, 1);
        private readonly object _clientLock = new();
        private readonly ConcurrentDictionary<string, int> _usageCounter = new();

        private volatile bool _isHealthy;
        private volatile bool _disposed;
        private DateTime _expireDate = DateTime.MaxValue;
        private DateTime _nextHealthCheckDate = DateTime.MinValue;
        private int _healthCheckCount = 2;
        private int _maxUsedCountPerDomain = -1;
        private ProxyZoneType _testerType = ProxyZoneType.Default;
        private ProxyEndpoint? _endpoint;
        private ProxyClientAdapter? _proxyClientAdapter;

        public Proxy ProxyInfo { get; protected set; } = null!;

        #region 协议相关
        public string Schema { get; private set; } = string.Empty;

        public string Host { get; private set; } = string.Empty;

        public int Port { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string Password { get; private set; } = string.Empty;
        #endregion

        public long UserId { get; protected set; }

        public string Id
        {
            get
            {
                if (ProxyInfo == null)
                    return string.Empty;

                return ProxyInfo.Id > 0 ? ProxyInfo.Id.ToString() : ProxyInfo.ObjectId;
            }
        }

        public bool IsExpired => _expireDate <= DateTime.UtcNow;

        /// <summary>
        /// 过期时间小于 30 分钟时按动态代理子节点处理。
        /// </summary>
        public bool IsDynamic => _expireDate - DateTime.UtcNow < TimeSpan.FromMinutes(30);

        public virtual bool ShouldHealthCheck
        {
            get
            {
                if (_disposed || ProxyInfo == null || !ProxyInfo.IsActive || IsExpired)
                    return false;

                if (IsDynamic && !_isHealthy && _healthCheckCount < 0)
                    return false;

                return DateTime.UtcNow >= _nextHealthCheckDate;
            }
        }

        public virtual bool IsEnable()
        {
            return !_disposed && ProxyInfo?.IsActive == true && _isHealthy && !IsExpired;
        }

        public virtual void MarkHealthless()
        {
            _isHealthy = false;
            _nextHealthCheckDate = DateTime.MinValue;
        }

        public virtual async Task<bool> HealthCheck()
        {
            if (!await _healthCheckLock.WaitAsync(0))
                return _isHealthy;

            try
            {
                if (_disposed || ProxyInfo == null || _endpoint == null || !ProxyInfo.IsActive)
                {
                    _isHealthy = false;
                    return false;
                }

                if (IsExpired)
                {
                    _isHealthy = false;
                    return false;
                }

                _nextHealthCheckDate = DateTime.UtcNow.Add(HealthCheckInterval);

                var validIpQueries = _iPQueries
                    .Where(x => x.Enable)
                    .Where(x => x.ProxyZoneType.HasFlag(_testerType))
                    .OrderBy(x => x.Order)
                    .ToList();
                if (validIpQueries.Count == 0)
                {
                    _logger.Error("没有可用的有效代理检测接口, 代理将变得不稳定,请联系开发者解决");
                    _isHealthy = _expireDate > DateTime.UtcNow && _isHealthy;
                    return _isHealthy;
                }

                foreach (var ipQuery in validIpQueries)
                {
                    var ipResult = await ipQuery.GetIP(ProxyInfo.Url);
                    if (ipResult.Ok)
                    {
                        _isHealthy = true;
                        _healthCheckCount = 2;
                        _logger.Debug($"代理 {Id} 检测结果: {_isHealthy}");
                        return true;
                    }
                }

                _isHealthy = false;
                _healthCheckCount--;
                _logger.Debug($"代理 {Id} 检测失败");
                return false;
            }
            catch (Exception ex)
            {
                _isHealthy = false;
                _healthCheckCount--;
                _logger.Warn($"代理 {Id} 健康检测异常");
                _logger.Warn(ex);
                return false;
            }
            finally
            {
                _healthCheckLock.Release();
            }
        }

        protected virtual void RecordUsage(string email)
        {
            var domain = GetEmailDomain(email);
            _usageCounter.AddOrUpdate(domain, 1, (_, count) => count + 1);
        }

        public override string ToString()
        {
            if (_endpoint == null)
                return string.Empty;

            if (string.IsNullOrEmpty(Username))
                return $"{Schema}://{Host}:{Port}";

            return $"{Schema}://{Username}:{Password}@{Host}:{Port}";
        }

        public virtual bool IsMatch(string email)
        {
            if (ProxyInfo == null)
                return false;

            if (!IsRegexMatch(email, ProxyInfo.MatchRegex))
                return false;

            if (_maxUsedCountPerDomain < 0)
                return true;

            var domain = GetEmailDomain(email);
            return !_usageCounter.TryGetValue(domain, out var usedCount)
                || usedCount < _maxUsedCountPerDomain;
        }

        public virtual async Task<ProxyClientAdapter?> GetProxyClientAsync(
            IServiceProvider serviceProvider,
            string email
        )
        {
            if (!IsEnable())
            {
                await HealthCheck();
                if (!IsEnable())
                    return null;
            }

            var endpoint = _endpoint;
            if (endpoint == null)
                return null;

            RecordUsage(email);

            lock (_clientLock)
            {
                _proxyClientAdapter ??= ProxyClientFactory.Create(this, endpoint, _logger);
                return _proxyClientAdapter;
            }
        }

        public virtual void Update(
            Proxy proxy,
            ProxyZoneType proxyZoneType = ProxyZoneType.Default,
            int expireSeconds = int.MaxValue,
            int maxUsedCountPerDomain = -1,
            long userId = 0
        )
        {
            ProxyInfo = proxy;
            _testerType = proxyZoneType;
            _expireDate =
                expireSeconds == int.MaxValue
                    ? DateTime.MaxValue
                    : DateTime.UtcNow.AddSeconds(expireSeconds);
            _maxUsedCountPerDomain = maxUsedCountPerDomain;
            _healthCheckCount = 2;
            _nextHealthCheckDate = DateTime.MinValue;
            _disposed = false;

            if (userId > 0)
                UserId = userId;

            if (!ProxyEndpoint.TryCreate(proxy, out var endpoint, out var errorMessage))
            {
                _logger.Error($"代理 {Id} 解析失败: {errorMessage}");
                MarkHealthless();
                return;
            }

            var connectionChanged = _endpoint == null || !_endpoint.HasSameConnection(endpoint!);
            _endpoint = endpoint;
            Schema = endpoint!.Scheme;
            Host = endpoint.Host;
            Port = endpoint.Port;
            Username = endpoint.Username;
            Password = endpoint.Password;

            if (connectionChanged)
            {
                lock (_clientLock)
                {
                    _proxyClientAdapter = null;
                }
                _usageCounter.Clear();
                MarkHealthless();
            }
        }

        public virtual void CleanupExpiredResources()
        {
            if (IsExpired)
                MarkHealthless();
        }

        public virtual void DisposeHandler()
        {
            if (_disposed)
                return;

            _disposed = true;
            _isHealthy = false;
            _usageCounter.Clear();
            lock (_clientLock)
            {
                _proxyClientAdapter = null;
            }
        }

        public static bool CanParse(string proxyString)
        {
            return ProxyEndpoint.CanParse(proxyString);
        }

        protected virtual void AutoHealthCheck()
        {
            // 健康检测由 ProxiesManager 集中调度，保留该方法避免破坏继承层级。
        }

        private static bool IsRegexMatch(string email, string? matchRegex)
        {
            if (string.IsNullOrEmpty(matchRegex))
                return true;

            try
            {
                return Regex.IsMatch(email, matchRegex);
            }
            catch
            {
                return false;
            }
        }

        private static string GetEmailDomain(string email)
        {
            var index = email.LastIndexOf('@');
            return index >= 0 && index < email.Length - 1 ? email[(index + 1)..] : email;
        }
    }
}
