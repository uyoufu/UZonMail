using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Proxies
{
    /// <summary>
    /// 单个代理
    /// </summary>
    public class SingleProxyFactory : IProxyFactory
    {
        public virtual int Order => 100;

        private static readonly List<string> _supportProtoco = ["http", "https", "socks4", "socks5"];

        public Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (string.IsNullOrWhiteSpace(proxy.Url)) return null;

            var protocol = proxy.Url.ToLower().Split("://")[0];
            if (!_supportProtoco.Contains(protocol)) return null;

            var handler = serviceProvider.GetRequiredService<ProxyHandler>();
            handler.Update(proxy);
            return Task.FromResult<IProxyHandler?>(handler);
        }
    }
}
