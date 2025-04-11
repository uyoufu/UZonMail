using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Emails;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Validators;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// Smtp 信息控制器
    /// </summary>
    public class SmtpInfoController(SmtpInfoService smtpInfo) : ControllerBaseV1
    {
        /// <summary>
        /// 根据邮箱推断 Smtp 信息
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("guess")]
        public async Task<ResponseResult<SmtpInfo>> GuessSmtpInfo(string email)
        {
            var isValid = email.IsValidEmail();
            if (!isValid)
            {
                return ResponseResult<SmtpInfo>.Fail("邮箱格式不正确");
            }

            var results = await smtpInfo.GuessSmtpInfos([email]);
            return results[email].ToSuccessResponse();
        }

        /// <summary>
        /// 批量推断 smtp 信息
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        [HttpPost("guess")]
        public async Task<ResponseResult<List<SmtpInfo>>> GuessSmtpInfo([FromBody] List<string> emails)
        {
            var results = await smtpInfo.GuessSmtpInfos(emails);
            return results.Values
                .DistinctBy(x => x.Host)
                .ToList()
                .ToSuccessResponse();
        }
    }
}
