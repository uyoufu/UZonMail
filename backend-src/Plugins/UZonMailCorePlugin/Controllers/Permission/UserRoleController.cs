using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Database.Validators;
using UZonMail.Core.Services.Permission;
using UZonMail.Core.Utils.Database;
using UZonMail.Core.Utils.Extensions;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.PagingQuery;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.DB.SQL.Core.Permission;
using UZonMail.DB.Utils;

namespace UZonMail.Core.Controllers.Permission
{
    public class UserRoleController(SqlContext db, PermissionService permission) : PermissionControllerBase
    {
        /// <summary>
        /// 创建或者更新角色
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<UserRoles>> UpsertUserRole([FromBody] UserRoles userRole)
        {
            var validator = new UserRoleValidator();
            var vdResult = validator.Validate(userRole);
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<UserRoles>();
            }

            // 查找 更新 roles
            var roleIds = userRole.Roles.Select(x => x.Id);
            userRole.Roles = await db.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync();
            if (userRole.Id > 0)
            {
                var existOne = await db.UserRole.Where(x => x.Id == userRole.Id)
                    .Include(x => x.Roles)
                    .FirstOrDefaultAsync();
                existOne.UserId = userRole.UserId;
                existOne.Roles.SetList(userRole.Roles);
                userRole = existOne;
            }
            else
            {
                db.UserRole.Add(userRole);
            }

            await db.SaveChangesAsync();
            return userRole.ToSuccessResponse();
        }

        /// <summary>
        /// 获取用户角色数量
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetRolesCount(string filter)
        {
            var dbSet = db.UserRole.AsNoTracking();
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.User.UserName.Contains(filter) || x.User.UserName.Contains(filter));
            }
            int count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取角色数据
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<UserRoles>>> GetRolesData(string filter, [FromBody] Pagination pagination)
        {
            var dbSet = db.UserRole.AsNoTracking();
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.User.UserName.Contains(filter) || x.User.UserName.Contains(filter));
            }
            var results = await dbSet.Page(pagination)
                .Include(x => x.User)
                .Include(x => x.Roles)
                .ToListAsync();

            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        [HttpDelete("{userRoleId:long}")]
        public async Task<ResponseResult<bool>> DeleteUserRole(long userRoleId)
        {
            // 先移除关联的角色
            var userRole = await db.UserRole.Where(x => x.Id == userRoleId)
                .Include(x => x.Roles)
                .FirstOrDefaultAsync();
            if (userRole == null) return false.ToFailResponse("未找到对应的用户角色");
            userRole.Roles.Clear();

            // 删除本身
            db.UserRole.Remove(userRole);
            await db.SaveChangesAsync();

            // 更新权限缓存
            var permissionCodesDic = await permission.UpdateUserPermissionsCache([userRole.UserId]);
            // 通知权限更新
            await permission.NotifyPermissionUpdate(permissionCodesDic);

            // 返回结果
            return true.ToSuccessResponse();
        }
    }
}
