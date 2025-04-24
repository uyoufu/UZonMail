using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.HandlerFactory
{
    /// <summary>
    /// 单个代理
    /// </summary>
    public class SingleProxyFactory : IProxyFactory
    {
        public virtual int Order => 100;

        private static readonly List<string> _supportProtoco = ["http", "https", "socks4", "socks5"];

        /// <summary>
        /// 接口中定义了的是异步方法
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (string.IsNullOrWhiteSpace(proxy.Url)) return Task.FromResult<IProxyHandler?>(null);

            var protocol = proxy.Url.ToLower().Split("://")[0];
            if (!_supportProtoco.Contains(protocol)) return Task.FromResult<IProxyHandler?>(null);

            var handler = serviceProvider.GetRequiredService<ProxyHandler>();
            handler.Update(proxy);

            return Task.FromResult<IProxyHandler?>(handler);
        }
    }
}
