using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UZonMail.Utils.Http
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 添加浏览器请求头
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static HttpRequestHeaders AddBrowserHeaders(this HttpRequestHeaders headers)
        {
            //headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36"));

            // 设置 User-Agent 请求头
            headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
            
            headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
            headers.Add("sec-ch-ua-mobile", "?0");
            headers.Add("sec-ch-ua-platform", "Windows");
            headers.Add("Accept", "*/*");
            headers.Add("Sec-Fetch-Site", "same-origin");
            headers.Add("Sec-Fetch-Mode", "cors");
            headers.Add("Sec-Fetch-Dest", "empty");
            headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            headers.Add("Host", "www.tiktok.com");
            headers.Add("Connection", "keep-alive");

            return headers;
        }
    }
}
