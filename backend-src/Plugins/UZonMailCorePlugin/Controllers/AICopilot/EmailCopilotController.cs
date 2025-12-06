using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Controllers.AICopilot.DTOs;
using UZonMail.CorePlugin.Services.AICopilot;
using UZonMail.CorePlugin.Services.AICopilot.Config;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Configs;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.AICopilot
{
    /// <summary>
    /// 邮件 AI 助手
    /// </summary>
    /// <param name="tokenService"></param>
    /// <param name="aiCopilot"></param>
    /// <param name="systemPrompts"></param>
    public class EmailCopilotController(
        SqlContext db,
        TokenService tokenService,
        AiCopilotService aiCopilot,
        IAppSettings<AiPrompts> systemPrompts,
        ILogger<EmailCopilotController> logger
    ) : ControllerBaseV1
    {
        /// <summary>
        /// generate email template based on prompt
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        [HttpPost("email/body/generation")]
        public async Task<ResponseResult<string>> GenerateEmailBody([FromBody] UserPromptData data)
        {
            var prompt = systemPrompts.Value.EmailBodyGeneration;
            if (string.IsNullOrEmpty(prompt))
            {
                return string.Empty.ToFailResponse("系统提示词未配置");
            }
            if (string.IsNullOrEmpty(data.UserPrompt))
            {
                return string.Empty.ToFailResponse("提示词为空");
            }

            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, prompt),
                new ChatMessage(ChatRole.User, data.UserPrompt)
            ];

            var userId = tokenService.GetUserSqlId();
            var result = await aiCopilot.AskOnceAsync(userId, messages);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 优化邮件正文
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("email/body/enhancement")]
        public async Task<ResponseResult<string>> EnhanceEmailBody([FromBody] EmailBodyData data)
        {
            if (string.IsNullOrEmpty(data.Body))
            {
                return string.Empty.ToFailResponse("邮件正文为空");
            }

            // 使用系统提示词对正文进行优化
            var prompt = systemPrompts.Value.EmailBodyEnhancement;
            if (string.IsNullOrEmpty(prompt))
            {
                return string.Empty.ToFailResponse("系统提示词未配置");
            }

            // 开始调用生成优化
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, prompt),
                new ChatMessage(ChatRole.User, data.Body)
            ];

            var userId = tokenService.GetUserSqlId();
            var result = await aiCopilot.AskOnceAsync(userId, messages);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 生成邮件主题建议
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("email/subjects/generation")]
        public async Task<ResponseResult<string>> GenerateEmailSubjectsWithBody(
            [FromBody] GenerateEmailSubjectsData data
        )
        {
            string emailBody = data.Body;
            if (string.IsNullOrEmpty(emailBody))
            {
                // 从模板中获取
                var template = await db
                    .EmailTemplates.Where(x => !x.IsDeleted)
                    .Where(x => x.Id == data.TemplateId)
                    .FirstOrDefaultAsync();
                if (template == null)
                    return string.Empty.ToFailResponse("Email Body is required");
                emailBody = template.Content;
            }

            // 使用系统提示词对正文进行优化
            var prompt = systemPrompts.Value.SubjectsSummary;
            if (string.IsNullOrEmpty(prompt))
            {
                return string.Empty.ToFailResponse("System prompt is required");
            }

            // 只保留 html 内容中的文本部分
            string pureText;
            try
            {
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(emailBody);
                pureText = htmlDoc.DocumentNode.InnerText;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "解析邮件正文为纯文本时出错，使用原始内容");
                return string.Empty.ToFailResponse("解析邮件正文出错");
            }
            // 开始调用生成优化
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, prompt),
                new ChatMessage(ChatRole.User, pureText)
            ];
            var userId = tokenService.GetUserSqlId();
            var result = await aiCopilot.AskOnceAsync(userId, messages);
            return result.ToSuccessResponse();
        }
    }
}
