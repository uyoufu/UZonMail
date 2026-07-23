using System.Net.Http;
using System.Threading.Tasks;
using UzonMail.Utils.Http.Request;
using UzonMail.Utils.Json;

namespace UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters
{
    /// <summary>
    /// 基于 http://httpbin.org/ip 实现的 IP 查询
    /// </summary>
    public class HttpBinIPQuery(HttpClient httpClient)
        : JsonParser(httpClient, ProxyZoneType.Default)
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
