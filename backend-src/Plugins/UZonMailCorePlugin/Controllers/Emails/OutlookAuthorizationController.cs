using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// Outlook 邮箱基于用户授权控制器
    /// 参考：https://github.com/jstedfast/MailKit/blob/master/ExchangeOAuth2.md
    /// </summary>
    public class OutlookAuthorizationController : ControllerBaseV1
    {
        /// <summary>
        /// 获取微软授权请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<string>> OnAuthorizationRequest()
        {
            return string.Empty.ToSuccessResponse();
        }

        /// <summary>
        /// 微软授权回调
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> OnAuthorizationCallback([FromQuery] string code)
        {
            return null;
        }
    }
}
