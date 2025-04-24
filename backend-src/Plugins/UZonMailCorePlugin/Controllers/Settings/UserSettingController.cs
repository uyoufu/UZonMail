using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Settings.Request;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    public class UserSettingController(SqlContext db, TokenService tokenService, SystemSettingService settingService) : ControllerBaseV1
    {
        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("string")]
        public async Task<ResponseResult<bool>> UpdateSystemSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            var userId = tokenService.GetUserSqlId();

            // 开始更新
            await settingService.UpdateSystemSetting(key, value, userId, type: SystemSettingType.User);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("json")]
        public async Task<ResponseResult<bool>> UpdateSystemSettingJson(string key, [FromBody] JToken value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            var userId = tokenService.GetUserSqlId();
            await settingService.UpdateSystemSettingJson(key, value, userId, type: SystemSettingType.User);
            return true.ToSuccessResponse();
        }

        [HttpPut("boolean")]
        public async Task<ResponseResult<bool>> UpdateSystemSetting(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            // 开始更新
            await settingService.UpdateSystemSetting(key, value, userId, type: SystemSettingType.User);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新long类型的系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("long")]
        public async Task<ResponseResult<bool>> UpdateSystemSetting(string key, long value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            // 开始更新
            await settingService.UpdateSystemSetting(key, value, userId, type: SystemSettingType.User);
            return true.ToSuccessResponse();
        }


        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        [HttpGet("kv")]
        public async Task<ResponseResult<SystemSetting?>> GetSystemSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return ResponseResult<SystemSetting?>.Fail("key不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            var settings = await settingService.GetSystemSetting(key, userId);
            return settings.ToSuccessResponse();
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        [HttpGet("kvs")]
        public async Task<ResponseResult<JObject>> GetSystemSettings(string keys)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return new JObject().ToFailResponse("keys不能为空");
            }
            var userId = tokenService.GetUserSqlId();
            var result = await settingService.GetSystemSettings(keys, userId);

            return result.ToSuccessResponse();
        }
    }
}
