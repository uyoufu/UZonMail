using log4net;
using log4net.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UZonMail.Utils.Json;

namespace UZonMail.Utils.Http.Request
{
    /// <summary>
    /// HttpRequestMessage 的 Fluent 接口
    /// </summary>
    public class FluentHttpRequest : HttpRequestMessage
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FluentHttpRequest));

        public FluentHttpRequest()
        {
        }

        public FluentHttpRequest(HttpMethod method, string url) : base(method, url)
        {
            _url = url;
        }

        private HttpClient _httpClient;
        public FluentHttpRequest WithHttpClient(HttpClient client)
        {
            _httpClient = client;
            return this;
        }

        private HttpClientHandler _httpClientHandler;

        /// <summary>
        /// 设置代理
        /// 设置代理时，会覆盖原来的 _httpClient
        /// </summary>
        /// <param name="proxyUrl"></param>
        /// <returns></returns>
        public FluentHttpRequest WithProxy(string proxyUrl)
        {
            if (string.IsNullOrEmpty(proxyUrl)) return this;

            _httpClientHandler ??= new HttpClientHandler();
            _httpClientHandler.WithProxy(proxyUrl);
            return this;
        }

        /// <summary>
        /// 指定请求方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public FluentHttpRequest WithMethod(HttpMethod method)
        {
            Method = method;
            return this;
        }

        private string _url;
        /// <summary>
        /// 添加请求 URL
        /// 若需要修改 params 参数，url 中的参数需要使用 {paramName} 的形式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public FluentHttpRequest WithUrl(string url)
        {
            _url = url;
            return this;
        }

        public readonly Dictionary<string, Parameter> _parameters = [];
        public List<Parameter> Parameters => _parameters.Values.ToList();

        public FluentHttpRequest AddQuery(string name, string value)
        {
            _parameters.Remove(name);
            var query = new UrlQuery(name, value);
            _parameters.Add(query.Key, query);
            return this;
        }

        public FluentHttpRequest AddParam(string name, string value)
        {
            _parameters.Remove(name);
            var param = new UrlParam(name, value);
            return this;
        }

        public FluentHttpRequest AddHeader(string name, string value)
        {
            Headers.Remove(name);
            Headers.Add(name, value);
            return this;
        }

        /// <summary>
        /// 通过参数构建 URL
        /// </summary>
        public Uri BuildUri()
        {
            ThrowErrorIfInvalid();

            // 将 params 进行替换
            var urlParams = _parameters.Values.Where(p => p.Type == ParameterType.Params);
            var strBuilder = new StringBuilder(_url);
            foreach (var urlParam in urlParams)
            {
                strBuilder = strBuilder.Replace($"{{{urlParam.Name}}}", urlParam.Value);
            }
            var urlBuilder = new UriBuilder(strBuilder.ToString());
            var queryCollection = HttpUtility.ParseQueryString(urlBuilder.Query);
            var urlQueries = _parameters.Values.Where(p => p.Type == ParameterType.Query);
            foreach (var urlQuery in urlQueries)
            {
                queryCollection[urlQuery.Name] = urlQuery.Value;
            }
            urlBuilder.Query = queryCollection.ToString();
            this.RequestUri = urlBuilder.Uri;
            return this.RequestUri;
        }

        private void ThrowErrorIfInvalid()
        {
            if (string.IsNullOrEmpty(_url)) throw new ArgumentNullException(nameof(_url));
        }

        /// <summary>
        /// 发送并返回响应
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync()
        {
            BuildUri();

            _httpClient ??= new HttpClient(_httpClientHandler);

            try
            {
                // 尝试发送请求
                return await _httpClient.SendAsync(this);
            }
            catch (HttpRequestException ex)
            {
                // 捕获网络异常并返回自定义的 HttpResponseMessage
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable, // 或其他适当的状态码
                    ReasonPhrase = $"Request failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // 捕获其他异常并返回自定义的 HttpResponseMessage
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = $"Unexpected error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 获取 Json 数据
        /// </summary>
        /// <returns></returns>
        public async Task<JObject?> GetJsonAsync()
        {
            var response = await SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warn($"请求失败：{response.StatusCode}, 消息: {response.ReasonPhrase}");
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            return content.JsonTo<JObject?>();
        }

        /// <summary>
        /// 获取带有 Reponse 的结果
        /// </summary>
        /// <returns></returns>
        public async Task<(JObject?, HttpResponseMessage)> GetJsonAsync2()
        {
            var response = await SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warn($"请求失败：{response.StatusCode}, 消息: {response.ReasonPhrase}");
                return (default, response);
            }
            var content = await response.Content.ReadAsStringAsync();
            return (content.JsonTo<JObject>(), response);
        }
    }
}
