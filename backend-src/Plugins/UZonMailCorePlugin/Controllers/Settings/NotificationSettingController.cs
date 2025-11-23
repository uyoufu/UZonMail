using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
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
            var emailSender = sendersManager.GetEmailSender(
                smtpSettings.Email,
                smtpSettings.SmtpHost
            );

            var userId = tokenService.GetUserSqlId();

            var outbox = new Outbox()
            {
                UserId = userId,
                Email = smtpSettings.Email,
                UserName = string.Empty,
                Password = encryptService.EncrytPassword(smtpSettings.Password),
                SmtpHost = smtpSettings.SmtpHost,
                SmtpPort = smtpSettings.SmtpPort,
                EnableSSL = true
            };
            // 开始验证
            var result = await emailSender.TestOutbox(serviceProvider, outbox);

            // 验证通过后，更新数据库
            smtpSettings.IsValid = result.Ok;

            // 保存到数据库
            var newSetting = await settingService.UpdateAppSetting(smtpSettings, type: type);

            // 更新缓存
            await settingsManager.ResetSetting<SmtpNotificationSetting>(newSetting.Id);

            if (!result.Ok)
                return false.ToFailResponse(result.Message);
            else
                return true.ToSuccessResponse();
        }
    }
}
