using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UZonMail.Utils.Http
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 先移除再添加请求头
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpClient TryAddHeader(this HttpClient httpClient, string name, string value)
        {
            httpClient.DefaultRequestHeaders.Remove(name);
            httpClient.DefaultRequestHeaders.Add(name, value);
            return httpClient;
        }
    }
}
