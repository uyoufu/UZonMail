using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Proxies
{
    /// <summary>
    /// 代理组
    /// </summary>
    public abstract class ProxyClusterFactory : IProxyFactory
    {
        public int Order => 0;

        public abstract IProxyHandler? CreateProxy(Proxy proxy);
    }
}
