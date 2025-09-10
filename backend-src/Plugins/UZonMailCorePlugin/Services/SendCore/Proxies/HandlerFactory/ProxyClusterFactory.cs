using UZonMail.Core.Services.SendCore.Proxies.Clients;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.Proxies.HandlerFactory
{
    /// <summary>
    /// 代理组
    /// </summary>
    public abstract class ProxyClusterFactory : IProxyFactory
    {
        public int Order => 0;

        public abstract Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy);
    }
}
