using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.Utils.Web.Access;

namespace UZonMail.Core.Services.Permission
{
    public class CoreAccessBuilder(SqlContext db) : IAccessBuilder
    {
        /// <summary>
        /// 生成用户的权限码
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task<Dictionary<long, List<string>>> GenerateUserPermissionCodes(List<long> userIds)
        {
            var userRoles = await db.UserRole.AsNoTracking()
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .GroupBy(x => x.UserId)
                .ToListAsync();

            Dictionary<long, List<string>> results = [];
            foreach (var item in userRoles)
            {
                var permissionCodes = item.SelectMany(x => x.Roles).SelectMany(x => x.PermissionCodes).Select(x => x.Code).Distinct().ToList();
                results.Add(item.Key, permissionCodes);
            }

            // 添加管理员权限码
            var users = await db.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.IsSuperAdmin, x.Type })
                .ToListAsync();

            foreach (var user in users)
            {
                if(!results.TryGetValue(user.Id,out var codes))
                {
                    codes = [];
                    results.Add(user.Id, codes);
                }

                if (user.IsSuperAdmin)
                    codes.AddRange(["admin", "*"]);

                // 如果是子账户，添加子账户权限码
                if (user.Type == UserType.SubUser)
                    codes.Add("subUser");
            }     
            
            return results;
        }
    }
}
