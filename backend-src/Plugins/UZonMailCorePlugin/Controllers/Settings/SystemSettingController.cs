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
    /// <summary>
    /// 系统级设置, 为 AppSetting 的一个子集
    /// </summary>
    /// <param name="db"></param>
    /// <param name="settingService"></param>
    public class SystemSettingController(SqlContext db, AppSettingService settingService) : ControllerBaseV1
    {
        [HttpPut("base-api-url")]
        public async Task<ResponseResult<bool>> UpdateBaseApiUrl([FromBody] UpdateBaseApiUrlBody dataParams)
        {
            var baseApiUrl = dataParams.BaseApiUrl;
            if (string.IsNullOrEmpty(baseApiUrl)) return false.ToFailResponse("baseUrl不能为空");

            // 开始更新
            var setting = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == AppSetting.BaseApiUrl);
            if (setting == null)
            {
                setting = new AppSetting
                {
                    Key = AppSetting.BaseApiUrl,
                    StringValue = baseApiUrl
                };
                db.AppSettings.Add(setting);
            }
            else
            {
                setting.StringValue = baseApiUrl;
            }
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

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

            // 开始更新
            await settingService.UpdateAppSetting(key, value);
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
            await settingService.UpdateAppSetting(key, value);
            return true.ToSuccessResponse();
        }

        [HttpPut("boolean")]
        public async Task<ResponseResult<bool>> UpdateSystemSetting(string key, bool value)
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
        public async Task<ResponseResult<bool>> UpdateSystemSetting(string key, long value)
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
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        [HttpGet("kv")]
        public async Task<ResponseResult<AppSetting?>> GetSystemSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return ResponseResult<AppSetting?>.Fail("key不能为空");
            }

            var settings = await settingService.GetAppSetting(key,AppSettingType.System);
            return settings.ToSuccessResponse();
        }
    }
}
