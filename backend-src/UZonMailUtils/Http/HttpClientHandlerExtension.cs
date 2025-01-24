using System;
using System.Net;
using System.Net.Http;

namespace UZonMail.Utils.Http
{
    public static class HttpClientHandlerExtension
    {
        public static HttpClientHandler WithProxy(this HttpClientHandler handler, string proxy)
        {
            return handler.WithProxy(new Uri(proxy));
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static HttpClientHandler WithProxy(this HttpClientHandler handler, Uri proxy)
        {
            var webProxy = new WebProxy()
            {
                Address = proxy,
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential()
            };
            handler.Proxy = webProxy;
            return handler;
        }
    }
}
