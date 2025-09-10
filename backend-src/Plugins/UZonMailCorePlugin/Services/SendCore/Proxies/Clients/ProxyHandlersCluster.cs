using log4net;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;

namespace UZonMail.Core.Services.SendCore.Proxies.Clients
{
    /// <summary>
    /// 代理客户端集群基类
    /// 动态代理使用该类
    /// 1. 动态代理按照域名进行数量控制
    /// </summary>
    public abstract class ProxyHandlersCluster : ProxyHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandlersCluster));
        private readonly ConcurrentDictionary<string, ProxyHandler> _handlers = [];

        /// <summary>
        /// 最小数量
        /// 当小于这个数量时，就会重新获取代理
        /// </summary>
        private readonly int _minimumCount = 1;

        /// <summary>
        /// 获取 IP 地址
        /// </summary>
        /// <returns></returns>
        protected abstract Task<List<ProxyHandler>> GetProxyHandlersAsync(IServiceProvider serviceProvider);

        /// <summary>
        /// 获取匹配的代理客户端
        /// </summary>
        /// <param name="scopeServiceProvider"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public override async Task<ProxyClientAdapter?> GetProxyClientAsync(IServiceProvider scopeServiceProvider, string email)
        {
            // 移除不可用的代理客户端
            DisposeHandler();

            // 判断是否有可用代理客户端，若没有，则更新
            if (_handlers.Count < _minimumCount)
            {
                var updateTask = UpdateProxyHandlers(scopeServiceProvider);
                if (_handlers.IsEmpty)
                {
                    // 若没有任何可用的代理，则等待更新完成
                    await updateTask;
                }
            }

            var domain = email.Split('@').Last();
            var ipRateLimiter = scopeServiceProvider.GetRequiredService<IPRateLimiter>();
            var settingsManager = scopeServiceProvider.GetRequiredService<AppSettingsManager>();
            var sqlContext = scopeServiceProvider.GetRequiredService<SqlContext>();
            var sendingSetting = await settingsManager.GetSetting<SendingSetting>(sqlContext, UserId);

            // 判断是否没有 email 可用的代理客户端
            var matchedHandler = _handlers.Values.Where(x => x.IsMatch(email) && x.IsEnable())
                .Where(x =>
                {
                    // 验证是否满足域名约束
                    // 若不满足约束，请求新的动态代理
                    return !ipRateLimiter.IsLimited(domain, x.Host, sendingSetting.MaxCountPerIPDomainHour);
                })
                .FirstOrDefault();
            if (matchedHandler == null)
            {
                // 说明没有匹配的可用的代理，重新获取
                await UpdateProxyHandlers(scopeServiceProvider);
            }

            if (_handlers.IsEmpty)
            {
                _logger.Warn("没有可用的代理客户端");
                return null;
            }

            // 选择第一个可用的代理
            matchedHandler = _handlers.Values.Where(x => x.IsMatch(email) && x.IsEnable())
                 .Where(x =>
                 {
                     // 验证是否满足域名约束
                     // 若不满足约束，请求新的动态代理
                     return !ipRateLimiter.IsLimited(domain, x.Host, sendingSetting.MaxCountPerIPDomainHour);
                 })
                .FirstOrDefault();
            if (matchedHandler == null)
            {
                _logger.Warn($"没有可用的代理客户端匹配 {email}");
                return null;
            }

            return await matchedHandler.GetProxyClientAsync(scopeServiceProvider, email);
        }

        private async Task UpdateProxyHandlers(IServiceProvider serviceProvider)
        {
            var handlers = await GetProxyHandlersAsync(serviceProvider);
            // 可能存在重复的代理
            foreach (var handler in handlers)
            {
                if (_handlers.TryGetValue(handler.Id, out var existOne))
                {
                    // 更新代理信息
                    existOne.Update(handler.ProxyInfo);
                }
                else
                {
                    // 首次添加时，对代理先进行健康检查
                    await handler.HealthCheck();
                    _handlers.TryAdd(handler.Id, handler);
                }
            }

            // 如果 _handlers 仍然为空，则说明没有获取到任何代理，标记为不可用
            if (_handlers.IsEmpty)
                MarkHealthless();
        }

        public override bool IsMatch(string email)
        {
            // 为空全部匹配
            if (string.IsNullOrEmpty(ProxyInfo.MatchRegex)) return true;

            // 规则匹配
            return Regex.IsMatch(email, ProxyInfo.MatchRegex);
        }

        /// <summary>
        /// 始终可用
        /// </summary>
        /// <returns></returns>
        public override bool IsEnable()
        {
            return !_isHealthless;
        }

        private bool _isHealthless = false;
        /// <summary>
        /// 标记非健康状态
        /// </summary>
        public override void MarkHealthless()
        {
            _isHealthless = true;
        }

        /// <summary>
        /// 动态代理集不进行健康状态检测
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> HealthCheck()
        {
            return true;
        }

        protected override void AutoHealthCheck()
        {
            // 不做任何操作
        }

        /// <summary>
        /// 在调用时会自动处理
        /// </summary>
        public override void DisposeHandler()
        {
            // 移除不可用的代理客户端
            var disabledHandlers = _handlers.Values.Where(x => !x.IsEnable());
            foreach (var disabledHandler in disabledHandlers)
            {
                _handlers.TryRemove(disabledHandler.Id, out _);
            }
        }

        #region 工具类
        /// <summary>
        /// 默认为 5 分钟
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected int GetExpireMinutes(string url)
        {
            var match = Regex.Match(url, "expireMinutes=(\\d+)");
            if (!match.Success)
                return 5;
            return int.Parse(match.Groups[1].Value);
        }

        protected string GetProtocol(string url)
        {
            var match = Regex.Match(url, "protocol=(socks4|socks5|http|https)");
            if (!match.Success)
                return "socks5";
            return match.Groups[1].Value;
        }
        #endregion
    }
}
