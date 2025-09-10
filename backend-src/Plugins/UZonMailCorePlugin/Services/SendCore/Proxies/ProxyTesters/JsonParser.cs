using Newtonsoft.Json.Linq;
using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.SendCore.Proxies.ProxyTesters
{
    /// <summary>
    /// json 解析器
    /// </summary>
    /// <param name="httpClient"></param>
    public abstract class JsonParser(HttpClient httpClient, ProxyZoneType testerType) : BaseProxyTester(httpClient, testerType)
    {
        protected override string? RetrieveIP(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            var json = content.JsonTo<JObject?>();
            if (json == null)
                return string.Empty;

            return json.SelectTokenOrDefault(GetJsonPathOfIP(), string.Empty);
        }

        /// <summary>
        /// 获取 IP 的 JSON 路径
        /// </summary>
        /// <returns></returns>
        protected virtual string GetJsonPathOfIP() => "ip";
    }
}
