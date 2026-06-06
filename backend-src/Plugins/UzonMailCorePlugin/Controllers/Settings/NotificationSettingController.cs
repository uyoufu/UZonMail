using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Settings
{
    /// <summary>
    /// 通知设置
    /// </summary>
    /// <param name="db"></param>
    public class NotificationSettingController(
        IServiceProvider serviceProvider,
        SqlContext db,
        AppSettingService settingService,
        TokenService tokenService,
        AppSettingsManager settingsManager,
        EmailSendersManager sendersManager,
        EncryptService encryptService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 获取发件通知设置
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ResponseResult<SmtpNotificationSetting>> GetSmtpNotificationSetting(
            AppSettingType type = AppSettingType.System
        )
        {
            // 获取发送设置
            var key = nameof(SmtpNotificationSetting);
            var settings = await settingService.GetAppSetting(key, type);
            if (settings == null)
            {
                return new SmtpNotificationSetting().ToSuccessResponse();
            }

            return settings.Json!.ToObject<SmtpNotificationSetting>()!.ToSuccessResponse();
        }

        /// <summary>
        /// 保存并验证通知邮箱
        /// </summary>
        /// <returns></returns>
        [HttpPut()]
        public async Task<ResponseResult<bool>> UpdateSmtpNotificationSetting(
            [FromBody] SmtpNotificationSetting smtpSettings,
            AppSettingType type = AppSettingType.System
        )
        {
            var emailSender = sendersManager.GetEmailSender(OutboxType.SMTP);

            var userId = tokenService.GetUserSqlId();

            var outbox = new Outbox()
            {
                UserId = userId,
                Email = smtpSettings.Email,
                UserName = string.Empty,
                Password = encryptService.EncrytPassword(smtpSettings.Password),
                SmtpHost = smtpSettings.SmtpHost,
                SmtpPort = smtpSettings.SmtpPort,
                //EnableSSL = true
                ConnectionSecurity = smtpSettings.ConnectionSecurity
            };
            // 开始验证
            var result = await emailSender.TestOutbox(serviceProvider, outbox);

            // 验证通过后，更新数据库
            smtpSettings.IsValid = result.Ok;
            smtpSettings.Status = result.Ok ? AppSettingStatus.Enabled : AppSettingStatus.Ignored;

            // 保存到数据库
            var newSetting = await settingService.UpdateAppSetting(smtpSettings, type: type);

            // 更新缓存
            await settingsManager.ResetSetting<SmtpNotificationSetting>(newSetting, db);

            if (!result.Ok)
                return false.ToFailResponse(result.Message);
            else
                return true.ToSuccessResponse();
        }
    }
}
