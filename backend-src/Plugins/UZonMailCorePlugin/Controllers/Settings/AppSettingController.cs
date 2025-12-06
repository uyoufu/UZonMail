using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Controllers.Settings.Validators;
using UZonMail.CorePlugin.Services.Permission;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.CorePlugin.Utils.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Settings
{
    /// <summary>
    /// 程序设置控制器
    /// </summary>
    /// <param name="db"></param>
    /// <param name="settingService"></param>
    /// <param name="tokenService"></param>
    /// <param name="permissionService"></param>
    /// <param name="settingsManager"></param>
    public class AppSettingController(
        SqlContext db,
        AppSettingService settingService,
        TokenService tokenService,
        PermissionService permissionService,
        AppSettingsManager settingsManager
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("string")]
        public async Task<ResponseResult<bool>> UpdateAppSetting(
            string key,
            string value,
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            await settingService.UpdateAppSetting(key, value, type);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("json")]
        public async Task<ResponseResult<bool>> UpdateAppSettingJson(
            string key,
            [FromBody] JToken value,
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            await settingService.UpdateAppSetting(key, value, type);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新bool类型的系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPut("boolean")]
        public async Task<ResponseResult<bool>> UpdateAppSetting(
            string key,
            bool value,
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            await settingService.UpdateAppSetting(key, value);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新long类型的系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("long")]
        public async Task<ResponseResult<bool>> UpdateAppSetting(
            string key,
            long value,
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            await settingService.UpdateAppSetting(key, value, type);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        [HttpGet("kv")]
        public async Task<ResponseResult<AppSetting?>> GetAppSetting(
            string key,
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return ResponseResult<AppSetting?>.Fail("key不能为空");
            }

            var settings = await settingService.GetAppSetting(key, type);
            return settings.ToSuccessResponse();
        }

        #region 发送设置
        /// <summary>
        /// 获取发件设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("sending-setting")]
        public async Task<ResponseResult<SendingSetting>> GetSendingSetting(
            AppSettingType type = AppSettingType.System
        )
        {
            // 获取发送设置
            var key = nameof(SendingSetting);
            var settings = await settingService.GetAppSetting(key, type);
            if (settings == null)
            {
                return new SendingSetting().ToSuccessResponse();
            }

            return settings.Json!.ToObject<SendingSetting>()!.ToSuccessResponse();
        }

        /// <summary>
        /// 更新组织设置
        /// 只有组织管理员可以更新这个设置
        /// </summary>
        /// <returns></returns>
        [HttpPut("sending-setting")]
        public async Task<ResponseResult<bool>> UpserSendingSetting(
            [FromBody] SendingSetting sendingSetting,
            AppSettingType type = AppSettingType.System
        )
        {
            // 进行数据验证
            var validator = new SendingSettingValidator();
            var vdResult = validator.Validate(sendingSetting);
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<bool>();
            }

            var userId = tokenService.GetUserSqlId();
            // 判断权限
            await settingService.CheckUpdatePermission(userId, type);

            var key = nameof(SendingSetting);
            var appSetting = await settingService.UpdateAppSetting(sendingSetting, key, type);

            // 更新缓存
            settingsManager.ResetSetting<SendingSetting>(appSetting);

            return true.ToSuccessResponse();
        }
        #endregion
    }
}
