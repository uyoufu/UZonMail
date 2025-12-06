using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Encrypt;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Settings
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

            // 若存在，则将其转换成设置对象，并将密码修改为 6 个 *
            var settingModel = settings.Json!.ToObject<AICopilotSetting>()!;
            if (!string.IsNullOrEmpty(settingModel.ApiKey))
                settingModel.ApiKey = encryptService.GetPasswordMask();

            return settingModel.ToSuccessResponse();
        }

        /// <summary>
        /// 保存 AI 设置
        /// </summary>
        /// <returns></returns>
        [HttpPut()]
        public async Task<ResponseResult<bool>> UpdateAICopilotSetting(
            [FromBody] AICopilotSetting copilotSetting,
            AppSettingType type = AppSettingType.System
        )
        {
            // 对 ApiKey 进行加密
            if (!encryptService.IsPasswordMask(copilotSetting.ApiKey))
            {
                copilotSetting.ApiKey = encryptService.EncrytPassword(copilotSetting.ApiKey);
            }
            else
            {
                // 从数据库中找到已经存在的密码并赋予
                var key = nameof(AICopilotSetting);
                var settings = await settingService.GetAppSetting(key, type);
                if (settings != null)
                {
                    var existingSettingModel = settings.Json!.ToObject<AICopilotSetting>()!;
                    copilotSetting.ApiKey = existingSettingModel.ApiKey;
                }
            }

            // 保存到数据库
            var newSetting = await settingService.UpdateAppSetting(copilotSetting, type: type);

            // 更新缓存
            settingsManager.ResetSetting<AICopilotSetting>(newSetting);

            return true.ToSuccessResponse();
        }
    }
}
