using UZonMail.Utils.Http.Request;

namespace UZonMail.Core.Services.SendCore.Proxies.ProxyTesters
{
    /// <summary>
    /// 百度网站连通性测试
    /// </summary>
    /// <param name="httpClient"></param>
    public class BaiduTester(HttpClient httpClient) : BaseProxyTester(httpClient,ProxyZoneType.Baidu)
    {
        public override int Order { get; } = -1000;

        private readonly string _apiUrl = "https://www.baidu.com";

        protected override FluentHttpRequest GetHttpRequestWithoutProxy()
        {
            return new FluentHttpRequest(HttpMethod.Get, _apiUrl);
        }

        protected override string? RetrieveIP(string content)
        {
            return string.Empty;
        }
    }
}
