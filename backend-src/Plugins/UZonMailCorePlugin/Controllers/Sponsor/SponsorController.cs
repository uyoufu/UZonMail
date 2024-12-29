using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Sponsor
{
    /// <summary>
    /// 赞助相关接口
    /// </summary>
    public class SponsorController(IHttpClientFactory httpClientFactory) : ControllerBaseV1
    {
        /// <summary>
        /// 获取赞助页面的 html
        /// </summary>
        /// <returns></returns>
        [HttpGet("content")]
        public async Task<ResponseResult<string>> GetSponsorPageHtml()
        {
            // 从 https://gitee.com/uzonmail/UZonMail/raw/master/docs/docs/sponsor.md 读取内容
            string url = "https://gitee.com/uzonmail/UZonMail/raw/master/docs/docs/sponsor.md";
            var httpClient = httpClientFactory.CreateClient();
            var content = await httpClient.GetStringAsync(url);

            // 去掉 --- yaml 头部
            content = content.Substring(content.LastIndexOf("---") + 3);
            return content.ToSuccessResponse();
        }
    }
}
