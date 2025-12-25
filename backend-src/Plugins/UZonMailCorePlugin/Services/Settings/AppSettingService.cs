using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.CorePlugin.Services.Permission;
using UZonMail.CorePlugin.Utils.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.Settings
{
    /// <summary>
    /// 系统通知设置
    /// </summary>
    public class AppSettingService(
        SqlContext db,
        TokenService tokenService,
        PermissionService permissionService
    ) : IScopedService
    {
        /// <summary>
        /// 检查设置更新权限
        /// 若不满足要求，抛出异常
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task CheckUpdatePermission(long userId, AppSettingType type)
        {
            if (type == AppSettingType.System)
            {
                // 只有系统管理员可以更新系统设置
                var isSuperAdmin = await permissionService.HasSuperAdminPermission(userId);
                if (!isSuperAdmin)
                    throw new KnownException("You are not super admin");
            }

            if (type == AppSettingType.Organization)
            {
                var isOrgAdmin = await permissionService.HasOrganizationPermission(userId);
                if (!isOrgAdmin)
                    throw new KnownException("You are not organization admin");
            }
        }

        private IQueryable<AppSetting> GetQueryableSetting(string key, AppSettingType type)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var queryResult = db.AppSettings.Where(x => x.Type == type && x.Key == key);
            if (type == AppSettingType.Organization)
            {
                queryResult = queryResult.Where(x =>
                    x.OrganizationId == tokenPayloads.OrganizationId
                );
            }

            if (type == AppSettingType.User)
            {
                queryResult = queryResult.Where(x => x.UserId == tokenPayloads.UserId);
            }

            return queryResult;
        }

        /// <summary>
        /// 更新系统设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<AppSetting> UpdateAppSetting(
            string key,
            string value,
            AppSettingType type = AppSettingType.System
        )
        {
            // 开始更新
            var setting = await GetQueryableSetting(key, type).FirstOrDefaultAsync();
            if (setting == null)
            {
                var tokenPayloads = tokenService.GetTokenPayloads();
                setting = new AppSetting
                {
                    UserId = tokenPayloads.UserId,
                    OrganizationId = tokenPayloads.OrganizationId,
                    Key = key,
                    StringValue = value,
                    Type = type
                };
                db.AppSettings.Add(setting);
            }
            else
            {
                setting.StringValue = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }

        public async Task<AppSetting> UpdateAppSetting(
            string key,
            JToken value,
            AppSettingType type = AppSettingType.System
        )
        {
            // 开始更新
            var setting = await GetQueryableSetting(key, type).FirstOrDefaultAsync();
            if (setting == null)
            {
                var tokenPayloads = tokenService.GetTokenPayloads();
                setting = new AppSetting
                {
                    UserId = tokenPayloads.UserId,
                    OrganizationId = tokenPayloads.OrganizationId,
                    Key = key,
                    Json = value,
                    Type = type
                };
                db.AppSettings.Add(setting);
            }
            else
            {
                setting.Json = value;
            }
            await db.SaveChangesAsync();
            return setting;
        }

        public async Task<AppSetting> UpdateAppSetting<T>(
            T settingModel,
            string key = "",
            AppSettingType type = AppSettingType.System
        )
        {
            if (string.IsNullOrEmpty(key))
                key = CacheKey.GetEntityKey<T>();

            // 更新
            var value = settingModel.ToJToken();
            return await UpdateAppSetting(key, value, type);
        }

        public async Task<AppSetting> UpdateAppSetting(
            string key,
            bool value,
            AppSettingType type = AppSettingType.System
        )
        {
            // 开始更新
            var setting = await GetQueryableSetting(key, type).FirstOrDefaultAsync();
            if (setting == null)
            {
                var tokenPayloads = tokenService.GetTokenPayloads();
                setting = new AppSetting
                {
                    UserId = tokenPayloads.UserId,
                    OrganizationId = tokenPayloads.OrganizationId,
                    Key = key,
                    BoolValue = value,
                    Type = type
                };
                db.AppSettings.Add(setting);
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
        public async Task<AppSetting> UpdateAppSetting(
            string key,
            long value,
            AppSettingType type = AppSettingType.System
        )
        {
            // 开始更新
            var setting = await GetQueryableSetting(key, type).FirstOrDefaultAsync();
            if (setting == null)
            {
                var tokenPayloads = tokenService.GetTokenPayloads();
                setting = new AppSetting
                {
                    UserId = tokenPayloads.UserId,
                    OrganizationId = tokenPayloads.OrganizationId,
                    Key = key,
                    LongValue = value,
                    Type = type
                };
                db.AppSettings.Add(setting);
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
        public async Task<AppSetting?> GetAppSetting(
            string key,
            AppSettingType type = AppSettingType.User
        )
        {
            return await GetQueryableSetting(key, type).FirstOrDefaultAsync();
        }
    }
}
