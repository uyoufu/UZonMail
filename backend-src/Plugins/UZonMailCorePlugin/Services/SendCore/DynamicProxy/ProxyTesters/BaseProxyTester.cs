
using log4net;
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Json;
using UZonMail.Utils.Results;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters
{
    /// <summary>
    /// 代理查询基类
    /// 来源：https://github.com/ihmily/ip-info-api?tab=readme-ov-file
    /// </summary>
    /// <param name="httpClient"></param>
    public abstract class BaseProxyTester : IProxyTester
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseProxyTester));

        private readonly HttpClient _httpClient;
        private readonly long _timeout = 5000;

        public BaseProxyTester(HttpClient httpClient,ProxyTesterType testerType)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMilliseconds(_timeout);
            TesterType = testerType;
        }

        public bool Enable { get; set; } = true;

        /// <summary>
        /// 序号
        /// </summary>
        public virtual int Order { get; } = 0;

        /// <summary>
        /// 代理查询类型
        /// </summary>
        public ProxyTesterType TesterType { get; private set; }

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
                .WithTimeout(_timeout)
                .WithProxy(proxyUrl)
                .SendAsync();

            // 未查找成功
            if (!response.IsSuccessStatusCode)
            {
                _logger.Debug($"动态IP检测失败,{response.StatusCode}: {response.ReasonPhrase}");
                return Result<string?>.Success(string.Empty);
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultIP = RetrieveIP(content);
            return Result<string?>.Success(resultIP);
        }

        protected abstract FluentHttpRequest GetHttpRequestWithoutProxy();

        /// <summary>
        /// 解析 IP
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected abstract string? RetrieveIP(string content);

        private DateTime _lastValidateTime = DateTime.MinValue;
        private bool _validating = false;
        /// <summary>
        /// 不使用代理验证可访问性
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<bool> Validate()
        {
            if (Interlocked.Exchange(ref _validating, true)) return false;

            _validating = true;
            _lastValidateTime = DateTime.Now;

            var response = await GetHttpRequestWithoutProxy()
                .WithHttpClient(_httpClient)
                .SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                Enable = false;
                _validating = false;
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                Enable = false;
                _validating = false;
                return false;
            }

            Enable = true;
            _validating = false;
            return true;
        }
    }
}
