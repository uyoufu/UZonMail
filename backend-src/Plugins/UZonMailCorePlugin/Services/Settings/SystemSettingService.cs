using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Settings.Models;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Settings
{
    /// <summary>
    /// 系统通知设置
    /// </summary>
    public class SystemSettingService(SqlContext db) : IScopedService
    {
        private static readonly string _systemSmtpNotificationSettingKey = "systemSmtpNotification";

        /// <summary>
        /// 获取系统 SMTP 通知设置
        /// </summary>
        /// <returns></returns>
        public async Task<SystemSmtpNotificationSetting> GetSmtpNotificationSetting()
        {
            var settingJson = await db.SystemSettings
                .Where(x => x.Key == _systemSmtpNotificationSettingKey)
                .Select(x => x.Json)
                .FirstOrDefaultAsync();

            // 不存在时，返回默认
            if(settingJson==null)return new SystemSmtpNotificationSetting();

            var setting = settingJson.ToObject<SystemSmtpNotificationSetting>();
            if (setting == null) return new SystemSmtpNotificationSetting();

            return setting;
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<SystemSetting> UpdateSystemSetting(string key, string value, long userId = 0, long orgId = 0, SystemSettingType type = SystemSettingType.System)
        {
            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key && x.UserId == userId && x.OrganizationId == orgId);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    UserId = userId,
                    OrganizationId = orgId,
                    Key = key,
                    StringValue = value,
                    Type = type
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.StringValue = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }

        public async Task<SystemSetting> UpdateSystemSettingJson(string key, JToken value, long userId = 0, long orgId = 0, SystemSettingType type = SystemSettingType.System)
        {
            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key && x.UserId == userId && x.OrganizationId == orgId);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    UserId = userId,
                    OrganizationId = orgId,
                    Key = key,
                    Json = value,
                    Type = type
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.Json = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }

        public async Task<SystemSetting> UpdateSystemSetting(string key, bool value, long userId = 0, long orgId = 0, SystemSettingType type = SystemSettingType.System)
        {
            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key && x.UserId == userId && x.OrganizationId == orgId);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    UserId = userId,
                    OrganizationId = orgId,
                    Key = key,
                    BoolValue = value,
                    Type = type
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.BoolValue = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }

        /// <summary>
        /// 更新long类型的系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("long")]
        public async Task<SystemSetting> UpdateSystemSetting(string key, long value, long userId = 0, long orgId = 0, SystemSettingType type = SystemSettingType.System)
        {
            // 开始更新
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key && x.UserId == userId && x.OrganizationId == orgId);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    UserId = userId,
                    OrganizationId = orgId,
                    Key = key,
                    LongValue = value,
                    Type = type
                };
                db.SystemSettings.Add(setting);
            }
            else
            {
                setting.LongValue = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }


        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        public async Task<SystemSetting?> GetSystemSetting(string key, long userId = 0, long orgId = 0)
        {
            return await db.SystemSettings.Where(x => x.Key == key && x.UserId == userId && x.OrganizationId == orgId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>设置的对象</returns>
        public async Task<JObject> GetSystemSettings(string keys, long userId = 0, long orgId = 0)
        {
            var keyList = keys.Split(',');
            var settings = await db.SystemSettings.Where(x => x.UserId == userId && x.OrganizationId == orgId && keyList.Contains(x.Key)).ToListAsync();
            var result = new JObject();
            foreach (var setting in settings)
            {
                result[setting.Key] = setting.StringValue;
            }

            return result;
        }
    }
}
