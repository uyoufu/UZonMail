using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Encrypt;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    public class AiSettingController(
        SqlContext db,
        AppSettingService settingService,
        AppSettingsManager settingsManager,
        EncryptService encryptService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 获取 AI 设置
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ResponseResult<AICopilotSetting>> GetAICopilotSetting(
            AppSettingType type = AppSettingType.System
        )
        {
            // 获取发送设置
            var key = nameof(AICopilotSetting);
            var settings = await settingService.GetAppSetting(key, type);
            if (settings == null)
            {
                return new AICopilotSetting().ToSuccessResponse();
            }

            return settings.Json!.ToObject<AICopilotSetting>()!.ToSuccessResponse();
        }

        /// <summary>
        /// 保存 AI 设置
        /// </summary>
        /// <returns></returns>
        [HttpPut()]
        public async Task<ResponseResult<bool>> UpdateAICopilotSetting(
            [FromBody] AICopilotSetting smtpSettings,
            AppSettingType type = AppSettingType.System
        )
        {
            // 对 ApiKey 进行加密
            smtpSettings.ApiKey = encryptService.EncrytPassword(smtpSettings.ApiKey);

            // 保存到数据库
            var newSetting = await settingService.UpdateAppSetting(smtpSettings, type: type);

            // 更新缓存
            settingsManager.ResetSetting<AICopilotSetting>(newSetting);

            return true.ToSuccessResponse();
        }
    }
}
