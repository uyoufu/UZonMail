using System;
using System.Net;
using System.Net.Http;

namespace UZonMail.Utils.Http
{
    /// <summary>
    /// HttpClientHandler 扩展
    /// </summary>
    public static class HttpClientHandlerExtension
    {
        /// <summary>
        /// 为 HttpClientHandler 设置代理
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static HttpClientHandler WithProxy(this HttpClientHandler handler, string proxy)
        {
            return handler.WithProxy(new Uri2(proxy));
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static HttpClientHandler WithProxy(this HttpClientHandler handler, Uri2 proxy)
        {
            var webProxy = new WebProxy()
            {
                Address = proxy,
                BypassProxyOnLocal = true,
                UseDefaultCredentials = false,
                Credentials = proxy.UserInfo2.GetCredential()
            };
            handler.Proxy = webProxy;
            return handler;
        }
    }
}
