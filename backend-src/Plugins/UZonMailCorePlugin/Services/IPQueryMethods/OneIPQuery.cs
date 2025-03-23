using UZonMail.Utils.Http.Request;

namespace UZonMail.Core.Services.IPQueryMethods
{
    public class OneIPQuery(HttpClient httpClient) : PlainTextParser(httpClient)
    {
        private readonly string _apiUrl = "https://1.1.1.1/cdn-cgi/trace";

        /// <summary>
        /// 最优先
        /// </summary>
        public override int Order { get; } = -2;

        protected override FluentHttpRequest GetHttpRequestWithoutProxy()
        {
            return new FluentHttpRequest(HttpMethod.Get, _apiUrl);
        }

        protected override string GetIpRegexMatchPrefix()
        {
            return "ip=";
        }
    }
}
