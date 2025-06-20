using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    /// <summary>
    /// 通知设置
    /// </summary>
    /// <param name="db"></param>
    public class NotificationSettingController(SqlContext db, AppSettingService settingService, AppSettingsManager settingsManager, EmailSendersManager sendersManager) : ControllerBaseV1
    {
        /// <summary>
        /// 获取发件通知设置
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ResponseResult<SmtpNotificationSetting>> GetSmtpNotificationSetting(AppSettingType type = AppSettingType.System)
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
        public async Task<ResponseResult<bool>> UpdateSmtpNotificationSetting([FromBody] SmtpNotificationSetting smtpSettings, AppSettingType type = AppSettingType.System)
        {
            var emailSender = sendersManager.GetEmailSender(smtpSettings.Email);

            // 开始验证
            var authenticateClient = emailSender.GetAuthenticateClient();
            var result = await authenticateClient.AuthenticateTestAsync(smtpSettings.Email, smtpSettings.Email, smtpSettings.Password, null, smtpSettings.SmtpHost, smtpSettings.SmtpPort, true);

            // 验证通过后，更新数据库
            smtpSettings.IsValid = result.Ok;

            // 保存到数据库
            var newSetting = await settingService.UpdateAppSetting(smtpSettings, type: type);

            // 更新缓存
            await settingsManager.ResetSetting<SmtpNotificationSetting>(newSetting.Id);

            if (!result.Ok) return false.ToFailResponse(result.Message);
            else return true.ToSuccessResponse();
        }
    }
}
