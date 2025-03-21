
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Json;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.IPQueryMethods
{
    /// <summary>
    /// IP 查询接口的基类
    /// 来源：https://github.com/ihmily/ip-info-api?tab=readme-ov-file
    /// </summary>
    /// <param name="httpClient"></param>
    public abstract class BaseIPQuery(HttpClient httpClient) : IIPQuery
    {
        public bool Enable { get; private set; } = true;

        /// <summary>
        /// 序号
        /// </summary>
        public virtual int Order { get; } = 0;

        /// <summary>
        /// 获取当前 IP
        /// </summary>
        /// <param name="proxyUrl"></param>
        /// <returns></returns>
        public async Task<Result<string?>> GetIP(string proxyUrl)
        {
            // 每天自动验证一次
            if (DateTime.Now.Date != _lastValidateTime.Date)
            {
                await Validate();
            }
            if (!Enable)
                return Result<string?>.Fail("IP 查询接口不可用");

            var response = await GetHttpRequestWithoutProxy()
                .WithProxy(proxyUrl)
                .SendAsync();

            // 未查找成功
            if (!response.IsSuccessStatusCode)
            {
                return Result<string?>.Success(string.Empty);
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultIP = IPParser(content);
            return Result<string?>.Success(resultIP);
        }

        protected abstract FluentHttpRequest GetHttpRequestWithoutProxy();

        /// <summary>
        /// 解析 IP
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected abstract string? IPParser(string content);

        private DateTime _lastValidateTime = DateTime.MinValue;
        /// <summary>
        /// 不使用代理验证可访问性
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            _lastValidateTime = DateTime.Now;

            var response = await GetHttpRequestWithoutProxy()
                .WithHttpClient(httpClient)
                .SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                Enable = false;
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                Enable = false;
                return false;
            }

            Enable = true;
            return true;
        }
    }
}
