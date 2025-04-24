using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Settings.Request;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Settings
{
    public class SystemSettingController(SqlContext db) : ControllerBaseV1
    {
        [HttpPut("base-api-url")]
        public async Task<ResponseResult<bool>> UpdateBaseApiUrl([FromBody] UpdateBaseApiUrlBody dataParams)
        {
            var baseApiUrl = dataParams.BaseApiUrl;
            if (string.IsNullOrEmpty(baseApiUrl)) return false.ToFailResponse("baseUrl不能为空");

            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == SystemSetting.BaseApiUrl);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = SystemSetting.BaseApiUrl,
                    StringValue = baseApiUrl
                };
                db.SystemSettings.Add(setting);
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
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    StringValue = value
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.StringValue = value;
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
        [HttpPut("json")]
        public async Task<ResponseResult<bool>> UpdateSystemSettingJson(string key, [FromBody]JObject value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false.ToFailResponse("key不能为空");
            }

            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    Json = value
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.Json = value;
            }
            await db.SaveChangesAsync();
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
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    BoolValue = value
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.BoolValue = value;
            }
            await db.SaveChangesAsync();
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
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    LongValue = value
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.LongValue = value;
            }
            await db.SaveChangesAsync();
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

            var settings = await db.SystemSettings.Where(x => x.Key == key).FirstOrDefaultAsync();
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

            var keyList = keys.Split(',');
            var settings = await db.SystemSettings.Where(x => keyList.Contains(x.Key)).ToListAsync();
            var result = new JObject();
            foreach (var setting in settings)
            {
                result[setting.Key] = setting.StringValue;
            }

            return result.ToSuccessResponse();
        }
    }
}
