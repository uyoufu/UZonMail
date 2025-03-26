using MailKit.Net.Proxy;
using System.Net;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Clients
{
    public class ProxyClientAdapter(IProxyHandler proxyHandler, IProxyClient proxyClient) : IProxyClient
    {
        #region 接口实现
        public NetworkCredential ProxyCredentials => proxyClient.ProxyCredentials;

        public string ProxyHost => proxyClient.ProxyHost;

        public int ProxyPort => proxyClient.ProxyPort;

        public IPEndPoint LocalEndPoint
        {
            get => proxyClient.LocalEndPoint;
            set => proxyClient.LocalEndPoint = value;
        }

        public Stream Connect(string host, int port, CancellationToken cancellationToken = default)
        {
            return proxyClient.Connect(host, port, cancellationToken);
        }

        public Stream Connect(string host, int port, int timeout, CancellationToken cancellationToken = default)
        {
            return proxyClient.Connect(host, port, timeout, cancellationToken);
        }

        public Task<Stream> ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            return proxyClient.ConnectAsync(host, port, cancellationToken);
        }

        public Task<Stream> ConnectAsync(string host, int port, int timeout, CancellationToken cancellationToken = default)
        {
            return proxyClient.ConnectAsync(host, port, timeout, cancellationToken);
        }
        #endregion

        #region 额外的接口
        public void MarkHealthless()
        {
            proxyHandler.MarkHealthless();
        }
        #endregion
    }
}
