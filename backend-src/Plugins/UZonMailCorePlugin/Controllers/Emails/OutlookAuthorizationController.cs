using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Emails.Requests;
using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.SendCore.Sender.MsGraph;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Emails
{
    /// <summary>
    /// Outlook 邮箱基于用户授权控制器
    /// 参考：https://github.com/jstedfast/MailKit/blob/master/ExchangeOAuth2.md
    /// TODO: 目前该方法存在验证问题
    /// </summary>
    public class OutlookAuthorizationController(IServiceProvider serviceProvider, SqlContext db, TokenService tokenService, EncryptService encryptService) : ControllerBaseV1
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OutlookAuthorizationController));

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

            var graphParams = serviceProvider.GetRequiredService<MsGraphParamsResolver>();
            graphParams.SetGraphInfo(outbox.UserName);

            var uri = serviceProvider.GetRequiredService<OutlookAuthorizationRequest>()
                .WithClientId(graphParams.ClientId)
                .WithState(outbox.ObjectId)
                .WithEmail(outbox.Email)
                .BuildUri();

            return uri.ToString().ToSuccessResponse();
        }

        private static string _outlookAuthorizeCallbackPage = string.Empty;
        /// <summary>
        /// 微软授权回调
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("code")]
        public async Task<IActionResult> OnAuthorizationCallback([FromQuery] string code, [FromQuery] string state)
        {
            var outbox = await db.Outboxes.FirstOrDefaultAsync(x => x.ObjectId == state);
            if (outbox == null)
            {
                return Content("未找到对应的发件箱", "text/html; charset=utf-8");
            }

            if (string.IsNullOrEmpty(_outlookAuthorizeCallbackPage))
            {
                // 从文件中读取
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                var smtpInfoPath = Path.Combine(assemblyDirectory, "data/init/outlookAuthorizeCallback.html");
                _outlookAuthorizeCallbackPage = await System.IO.File.ReadAllTextAsync(smtpInfoPath);
            }

            // 解析用户名和密码
            var plainPassword = encryptService.DecryptOutboxSecret(outbox.UserId, outbox.Password);
            var graphParams = serviceProvider.GetRequiredService<MsGraphParamsResolver>();
            graphParams.SetGraphInfo(outbox.UserName, plainPassword);

            var request = serviceProvider.GetRequiredService<OutlookRefreshokenRequest>()
               .WithFormData(graphParams.ClientId, graphParams.ClientSecret, code);

            var response = await request.SendAsync();
            var result = new JObject()
            {
                { "ok",response.IsSuccessStatusCode},
                { "message",response.ReasonPhrase}
            };
            var content = _outlookAuthorizeCallbackPage.Replace("{{ authorizeResult }}", $"JSON.parse('{result.ToJson()}')");

            if (response.IsSuccessStatusCode)
            {
                // 保存结果
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = responseContent.JsonTo<AuthenticationResult2>();

                graphParams.SetRefreshToken(authResult?.RefreshToken);
                
                var encryptedPassword = encryptService.EncryptOutboxSecret(outbox.UserId, graphParams.GetPasswordForDB());
                outbox.Password = encryptedPassword;
                outbox.Status = OutboxStatus.Valid;
                await db.SaveChangesAsync();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.Warn($"授权失败: {responseContent}");
            }

            return Content(content, "text/html; charset=utf-8");
        }
    }
}
