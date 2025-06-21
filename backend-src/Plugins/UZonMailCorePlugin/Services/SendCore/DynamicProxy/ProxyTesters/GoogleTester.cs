using UZonMail.Utils.Http.Request;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters
{
    /// <summary>
    /// Google 网站连通性测试
    /// </summary>
    /// <param name="httpClient"></param>
    public class GoogleTester(HttpClient httpClient) : BaseProxyTester(httpClient, ProxyTesterType.Google)
    {
        public override int Order { get; } = -1;

        private readonly string _apiUrl = "https://www.google.com";

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
