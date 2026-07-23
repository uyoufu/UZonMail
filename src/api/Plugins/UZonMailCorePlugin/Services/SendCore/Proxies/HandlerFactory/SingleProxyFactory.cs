using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.HandlerFactory
{
    /// <summary>
    /// 单个代理
    /// </summary>
    public class SingleProxyFactory : IProxyFactory
    {
        public virtual int Order => 100;

        /// <summary>
        /// 接口中定义了的是异步方法
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (!ProxyEndpoint.CanParse(proxy.Url))
                return Task.FromResult<IProxyHandler?>(null);

            var handler = serviceProvider.GetRequiredService<ProxyHandler>();
            handler.Update(proxy);

            return Task.FromResult<IProxyHandler?>(handler);
        }
    }
}
