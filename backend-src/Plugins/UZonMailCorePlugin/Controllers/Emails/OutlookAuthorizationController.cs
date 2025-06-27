using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Emails.Requests;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// Outlook 邮箱基于用户授权控制器
    /// 参考：https://github.com/jstedfast/MailKit/blob/master/ExchangeOAuth2.md
    /// TODO: 目前该方法存在验证问题
    /// </summary>
    public class OutlookAuthorizationController(IServiceProvider serviceProvider, SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取微软授权请求
        /// </summary>
        /// <returns></returns>
        [HttpPost("{outboxId:long}")]
        public async Task<ResponseResult<string>> OnAuthorizationRequest(long outboxId)
        {
            var userId = tokenService.GetUserSqlId();

            // 查找 outbox
            var outbox = await db.Outboxes.Where(x => x.Id == outboxId && x.UserId == userId).FirstOrDefaultAsync();
            if (outbox == null)
            {
                return string.Empty.ToFailResponse("未找到该发件箱");
            }

            var uri = serviceProvider.GetRequiredService<OutlookAuthorizationRequest>()
                .WithClientId(outbox.UserName)
                .WithState(outbox.Id)
                .BuildUri();

            return uri.ToString().ToSuccessResponse();
        }

        /// <summary>
        /// 微软授权回调
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("code")]
        public async Task<IActionResult> OnAuthorizationCallback([FromQuery] string code, [FromQuery] long state)
        {
            return null;
        }
    }
}
