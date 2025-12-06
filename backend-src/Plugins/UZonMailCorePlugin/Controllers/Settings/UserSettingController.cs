using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Controllers.Settings.Request;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.CorePlugin.Controllers.Settings
{
    /// <summary>
    /// 用户级设置
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    /// <param name="settingService"></param>
    public class UserSettingController(
        SqlContext db,
        TokenService tokenService,
        AppSettingService settingService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("string")]
        public async Task<ResponseResult<bool>> UpdateUserSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            var userId = tokenService.GetUserSqlId();

            // 开始更新
            await settingService.UpdateAppSetting(key, value, type: AppSettingType.User);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("json")]
        public async Task<ResponseResult<bool>> UpdateUserSettingJson(
            string key,
            [FromBody] JToken value
        )
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            var userId = tokenService.GetUserSqlId();
            await settingService.UpdateAppSetting(key, value, type: AppSettingType.User);
            return true.ToSuccessResponse();
        }

        [HttpPut("boolean")]
        public async Task<ResponseResult<bool>> UpdateUserSetting(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            // 开始更新
            await settingService.UpdateAppSetting(key, value, type: AppSettingType.User);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新long类型的系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("long")]
        public async Task<ResponseResult<bool>> UpdateUserSetting(string key, long value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            // 开始更新
            await settingService.UpdateAppSetting(key, value, AppSettingType.User);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        [HttpGet("kv")]
        public async Task<ResponseResult<AppSetting?>> GetUserSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return ResponseResult<AppSetting?>.Fail("key不能为空");
            }

            var settings = await settingService.GetAppSetting(key, AppSettingType.User);
            return settings.ToSuccessResponse();
        }
    }
}
