using System.Net.Http;
using System.Threading.Tasks;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters
{
    /// <summary>
    /// 基于 http://httpbin.org/ip 实现的 IP 查询
    /// </summary>
    public class HttpBinIPQuery(HttpClient httpClient) : JsonParser(httpClient, ProxyTesterType.All)
    {
        private readonly string _apiUrl = "http://httpbin.org/ip";

        /// <summary>
        /// 最优先
        /// </summary>
        public override int Order { get; } = -1;

        protected override FluentHttpRequest GetHttpRequestWithoutProxy()
        {
            return new FluentHttpRequest(HttpMethod.Get, _apiUrl);
        }

        protected override string GetJsonPathOfIP()
        {
            return "origin";
        }
    }
}
