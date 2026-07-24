using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.HandlerFactory
{
    /// <summary>
    /// 代理组
    /// </summary>
    public abstract class ProxyClusterFactory : IProxyFactory
    {
        public abstract string Kind { get; }

        public int Order => 0;

        public abstract bool CanHandle(Uri uri);

        public abstract Task<IProxyHandler?> CreateProxy(
            IServiceProvider serviceProvider,
            Proxy proxy
        );
    }
}
