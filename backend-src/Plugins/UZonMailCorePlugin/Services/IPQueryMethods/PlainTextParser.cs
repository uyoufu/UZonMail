using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UZonMail.Utils.Json;

namespace UZonMail.Core.Services.IPQueryMethods
{
    /// <summary>
    /// 纯文本解析器
    /// </summary>
    /// <param name="httpClient"></param>
    public abstract class PlainTextParser(HttpClient httpClient) : BaseIPQuery(httpClient)
    {
        protected override string? IPParser(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // 使用正则表达式进行匹配
            var regex = new Regex(GetIpRegexMatchPrefix() + @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(content);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取 IP 的 JSON 路径
        /// </summary>
        /// <returns></returns>
        protected virtual string GetIpRegexMatchPrefix() => "ip=";
    }
}
