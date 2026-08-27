using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Settings
{
    /// <summary>
    /// 通知设置
    /// </summary>
    public class NotificationSettingController(
        IServiceProvider serviceProvider,
        AppSettingService settingService,
        TokenService tokenService,
        EmailSendersManager sendersManager
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
            var emailSender = sendersManager.GetEmailSender(SendingProtocol.Smtp);

            var userId = tokenService.GetUserSqlId();

            var senderAccount = new SenderAccount()
            {
                EmailAccount = new EmailAccount { UserId = userId, Email = smtpSettings.Email, },
                Protocol = SendingProtocol.Smtp,
            };
            var runtimeAccount = new SenderEmailAddress(
                senderAccount,
                new SenderCredentialSnapshot(
                    new SmtpCredentialSnapshot(
                        smtpSettings.SmtpHost,
                        smtpSettings.SmtpPort,
                        smtpSettings.ConnectionSecurity,
                        smtpSettings.Email,
                        smtpSettings.Password
                    ),
                    null
                ),
                0,
                SenderEmailAddressType.Shared
            );
            // 开始验证
            var result = await emailSender.ValidateAsync(serviceProvider, runtimeAccount);

            // 验证通过后，更新数据库
            smtpSettings.IsValid = result.IsSuccess;
            smtpSettings.Status = result.IsSuccess
                ? AppSettingStatus.Enabled
                : AppSettingStatus.Ignored;

            // 保存到数据库
            await settingService.UpdateAppSetting(smtpSettings, type: type);

            if (!result.IsSuccess)
                return false.ToFailResponse(result.Message);
            else
                return true.ToSuccessResponse();
        }
    }
}
