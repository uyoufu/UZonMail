using log4net;
using MailKit.Net.Proxy;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.Clients
{
    public static class ProxyClientFactory
    {
        public static ProxyClientAdapter? Create(
            IProxyHandler proxyHandler,
            ProxyEndpoint endpoint,
            ILog logger
        )
        {
            IProxyClient proxyClient = endpoint.Scheme switch
            {
                "socks5" => new Socks5Client(endpoint.Host, endpoint.Port, endpoint.Credentials),
                "http" => new HttpProxyClient(endpoint.Host, endpoint.Port, endpoint.Credentials),
                "https" => new HttpsProxyClient(endpoint.Host, endpoint.Port, endpoint.Credentials),
                "socks4" => new Socks4Client(endpoint.Host, endpoint.Port, endpoint.Credentials),
                "socks4a" => new Socks4aClient(endpoint.Host, endpoint.Port, endpoint.Credentials),
                _ => null!,
            };

            if (proxyClient == null)
            {
                logger.Error($"不支持的代理协议: {endpoint.Scheme}");
                return null;
            }

            return new ProxyClientAdapter(proxyHandler, proxyClient);
        }
    }
}
