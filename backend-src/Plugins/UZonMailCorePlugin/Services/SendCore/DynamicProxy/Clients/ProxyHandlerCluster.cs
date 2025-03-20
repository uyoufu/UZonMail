using log4net;
using MailKit.Net.Proxy;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Clients
{
    /// <summary>
    /// 代理客户端集群基类
    /// </summary>
    public abstract class ProxyHandlerCluster(Proxy proxy) : ProxyHandler(proxy)
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProxyHandlerCluster));
        private List<ProxyHandler> _handlers = [];

        /// <summary>
        /// 获取 IP 地址
        /// </summary>
        /// <returns></returns>
        protected abstract Task<List<ProxyHandler>> GetProxyHandlersAsync();

        public override async Task<IProxyClient?> GetProxyClientAsync(string matchStr)
        {
            // 移除不可用的代理客户端
            _handlers.RemoveAll(handler => !handler.IsEnable());

            // 判断是否有可用代理客户端，若没有，则更新
            if (_handlers.Count == 0)
            {
                var handlers = await GetProxyHandlersAsync();
                _handlers.AddRange(handlers);
            }

            if (_handlers.Count == 0)
            {
                _logger.Warn("没有可用的代理客户端");
                return null;
            }

            // 随机选择一个代理客户端
            var handler = _handlers[new Random().Next(0, _handlers.Count)];
            return await handler.GetProxyClientAsync(matchStr);
        }

        public override bool IsMatch(string matchStr, int limitCount)
        {
            if (string.IsNullOrEmpty(ProxyInfo.MatchRegex)) return true;

            // 规则匹配
            return Regex.IsMatch(matchStr, ProxyInfo.MatchRegex);
        }
    }
}
