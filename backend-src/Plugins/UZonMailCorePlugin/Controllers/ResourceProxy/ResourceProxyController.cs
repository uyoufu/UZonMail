using Microsoft.AspNetCore.Mvc;

namespace UZonMail.CorePlugin.Controllers.ResourceProxy
{
    /// <summary>
    /// 资源代理控制器
    /// </summary>
    public class ResourceProxyController(HttpClient httpClient) : ControllerBaseV1
    {
        /// <summary>
        /// 代理资源
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        [HttpGet("stream")]
        public async Task ProxyStream(string uri)
        {
            // 直接转发
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                Response.ContentType =
                    response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                await stream.CopyToAsync(Response.Body);
            }
            else
            {
                Response.StatusCode = (int)response.StatusCode;
            }
        }
    }
}
