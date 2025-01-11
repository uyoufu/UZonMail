using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.Token;
using Microsoft.Extensions.Options;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.Core.Services.Permission;
using UZonMail.Core.Config;
using Uamazing.Utils.Web.Token;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Core.Services.Plugin;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL.Permission;
using UZonMail.DB.Managers.Cache;
using System.Security.Claims;

namespace UZonMail.Core.Services.UserInfos
{
    /// <summary>
    /// 只在请求生命周期内有效的服务
    /// </summary>
    public class UserService(IServiceProvider serviceProvider, SqlContext db, IOptions<AppConfig> appConfig, PermissionService permission,
        PluginService pluginService, TokenService tokenService) : IScopedService
    {
        /// <summary>
        /// 判断用户是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> ExistUser(string userId)
        {
            return await db.Users.FirstOrDefaultAsync(x => x.UserId.ToLower() == userId.ToLower()) != null;
        }

        /// <summary>
        /// 创建用户的默认部门和组织
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parentOrganizationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<Tuple<Department, Department>> CreateUserHomeDepartments(SqlContext ctx, long parentOrganizationId, string userId)
        {
            var orgName = $"Org-{userId}";
            var departmentName = $"Department-{userId}";

            var organization = await ctx.Departments.FirstOrDefaultAsync(x => x.ParentId == parentOrganizationId
            && x.Name == orgName
            && x.Type == DepartmentType.Organization);
            if (organization == null)
            {
                var parentOrganization = await ctx.Departments.FirstOrDefaultAsync(x => x.Id == parentOrganizationId);
                // 在当前用户组织下创建新的组织
                organization = new Department()
                {
                    ParentId = parentOrganizationId,
                    Name = orgName,
                    Description = $"{userId}的组织",
                    Type = DepartmentType.Organization,
                    FullPath = $"{parentOrganization.FullPath}/{orgName}"
                };
                ctx.Add(organization);
                await ctx.SaveChangesAsync();
            }

            var department = await ctx.Departments.FirstOrDefaultAsync(x => x.ParentId == organization.Id
              && x.Name == departmentName
              && x.Type == DepartmentType.Department);
            if (department == null)
            {
                // 在组织中创建部门
                department = new Department()
                {
                    ParentId = organization.Id,
                    Name = departmentName,
                    Description = $"{userId}的部门",
                    Type = DepartmentType.Department,
                    FullPath = $"{organization.FullPath}/{orgName}"
                };
                ctx.Add(department);
                await ctx.SaveChangesAsync();
            }

            return new Tuple<Department, Department>(organization, department);
        }

        /// <summary>
        /// 添加组织管理员
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task AssignOrganizationAdminRole(SqlContext ctx, long userId)
        {
            // 获取超管对应的组织管理员角色
            var adminUserRole = await ctx.UserRole.Include(x => x.User)
                .Where(x => x.User.IsSuperAdmin)
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .FirstOrDefaultAsync();
            if (adminUserRole == null) return;

            // 获取超管角色
            var orgRole = adminUserRole.Roles.Where(x => x.PermissionCodes.Any(y => y.Code == PermissionCode.OrganizationPermissionCode)).FirstOrDefault();
            if (orgRole == null) return;

            // 为用户添加组织管理员角色
            var userRole = await ctx.UserRole.Where(x => x.UserId == userId)
                .Include(x => x.Roles)
                .FirstOrDefaultAsync();
            if (userRole == null)
            {
                userRole = new UserRoles()
                {
                    UserId = userId,
                };
                ctx.Add(userRole);
            }

            if (!userRole.Roles.Any(x => x.Equals(orgRole)))
            {
                userRole.Roles.Add(orgRole);
            }
            await ctx.SaveChangesAsync();
        }

        /// <summary>
        /// 移除组织管理员
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task RemoveOrganizationAdminRole(SqlContext ctx, long userId)
        {
            var userRole = await ctx.UserRole.Where(x => x.UserId == userId)
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .FirstOrDefaultAsync();
            var orgRoles = userRole.Roles.Where(x => x.PermissionCodes.Any(y => y.Code == PermissionCode.OrganizationPermissionCode)).ToList();
            foreach (var role in orgRoles)
            {
                userRole.Roles.Remove(role);
            }
            await ctx.SaveChangesAsync();
        }

        /// <summary>
        /// 创建一个新的用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password">原密码,不需要加密</param>
        /// <returns></returns>
        public async Task<User> CreateUser(string userId, string password)
        {
            var organizationId = tokenService.GetOrganizationId();
            return await db.RunTransaction(async ctx =>
             {
                 var (organization, department) = await CreateUserHomeDepartments(ctx, organizationId, userId);
                 var user = new User()
                 {
                     OrganizationId = organization.Id,
                     DepartmentId = department.Id,
                     UserId = userId,
                     Password = password.Sha256(1),
                     Type = UserType.Independent,
                     CreateBy = tokenService.GetUserSqlId(),
                 };
                 ctx.Add(user);
                 await ctx.SaveChangesAsync();

                 // 为用户分配组织管理员
                 await AssignOrganizationAdminRole(ctx, user.Id);

                 return user;
             });
        }

        /// <summary>
        /// 更新用户的类型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<bool> UpdateUserType(long userId, UserType type)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId) ?? throw new KnownException("用户不存在");
            if (user.Type == type) return true;

            // 只能修改由自己创建的用户
            var handlerId = tokenService.GetUserSqlId();
            if (user.CreateBy != handlerId) throw new KnownException("只能操作由自己创建的用户");

            user.Type = type;
            var organizationId = tokenService.GetOrganizationId();

            if (type == UserType.SubUser)
            {
                // 放到自己组织下               
                var department = await db.Departments.FirstOrDefaultAsync(x => x.ParentId == organizationId && x.Type == DepartmentType.Department)
                    ?? throw new KnownException("部门不存在");
                user.OrganizationId = organizationId;
                user.DepartmentId = department.Id;

                // 移除组织管理员权限
                await RemoveOrganizationAdminRole(db, user.Id);
            }
            else
            {
                // 放到其原来的组织下
                var (organization, department) = await CreateUserHomeDepartments(db, organizationId, user.UserId);
                user.OrganizationId = organization.Id;
                user.DepartmentId = department.Id;

                // 为用户分配组织管理员
                await AssignOrganizationAdminRole(db, user.Id);
            }
            await db.SaveChangesAsync();

            // 更新用户的组织设置和和退订设置
            DBCacher.SetCacheDirty<UserInfoCache>(user.Id.ToString());

            return true;
        }

        /// <summary>
        /// 通过用户 ID 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserInfo(int userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId) ?? throw new KnownException(userId + "用户不存在");
            // 将密码置空
            user.Password = string.Empty;
            return user;
        }

        /// <summary>
        /// 通过 userId 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<User> GetUserInfo(string userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new KnownException(userId + "用户不存在");
            // 将密码置空
            user.Password = string.Empty;
            return user;
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password">密码为原值</param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<UserSignInResult> UserSignIn(string userId, string password)
        {
            password = password.Sha256();
            User user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId && x.Password == password)
                ?? throw new KnownException("用户名或密码错误");

            // 禁用，则返回错误
            if (user.Status == UserStatus.ForbiddenLogin)
                throw new KnownException("该账号已注销");

            var userInfo = new User()
            {
                Id = user.Id,
                Avatar = user.Avatar,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                DepartmentId = user.DepartmentId,
                Type = user.Type,
                Status = user.Status,
            };

            // 生成 token
            string token = await GenerateToken(user);

            // 查找用户的权限
            List<string> access = await permission.GetUserPermissionCodes(user.Id);
            // 获取插件中的权限码

            // 获取已经安装的插件名称
            var installedPlugins = pluginService.GetInstalledPluginNames();

            return new UserSignInResult()
            {
                Token = token,
                Access = access.Distinct().ToList(),
                UserInfo = userInfo,
                InstalledPlugins = installedPlugins
            };
        }



        /// <summary>
        /// 生成 token
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private async Task<string> GenerateToken(User userInfo)
        {
            var tokenClaimBuilders = serviceProvider.GetServices<ITokenClaimBuilder>();
            List<Claim> claims = [];
            foreach(var claimBuilder in tokenClaimBuilders)
            {
                var claimsTemp = await claimBuilder.Build(userInfo);
                claims.AddRange(claimsTemp);
            }
            string token = JWTToken.CreateToken(appConfig.Value.TokenParams, claims);
            return token;
        }

        /// <summary>
        /// 获取用户数量
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetFilteredUsersCount(string filter)
        {
            return await FilterUser(filter).CountAsync();
        }
        private IQueryable<User> FilterUser(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            // 只显示由自己创建的账户
            return db.Users.Where(x => !x.IsDeleted && !x.IsHidden && !x.IsSuperAdmin)
                .Where(x => x.CreateBy == userId)
                .Where(x => string.IsNullOrEmpty(filter) || x.UserId.Contains(filter));
        }

        /// <summary>
        /// 获取分页的用户
        /// 返回除了密码之外的所有信息
        /// 调用接口对数据进行再处理
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetFilteredUsersData(string filter, Pagination pagination)
        {
            return await FilterUser(filter).Page(pagination).ToListAsync();
        }

        /// <summary>
        /// 获取用户默认密码
        /// </summary>
        /// <returns></returns>
        public string GetUserDefaultPassword()
        {
            return appConfig.Value.User.DefaultPassword;
        }

        /// <summary>
        /// 重置用户密码
        /// 由于无法知道用户的原始密码，因此重置后，发件箱的 smtp 密码将无法被解密
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<bool> ResetUserPassword(string userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new KnownException("用户不存在");
            user.Password = appConfig.Value.User.DefaultPassword.Sha256(1);
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> ChangeUserPassword(long userId, ChangePasswordModel passwordModel)
        {
            var oldPassword = passwordModel.OldPassword;
            var newPassword = passwordModel.NewPassword;

            if (userId <= 0 || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                throw new KnownException("新旧密码不能为空");
            }

            // 查找用户
            string encryptOldPassword = oldPassword.Sha256();
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Password == encryptOldPassword) ?? throw new KnownException("原密码错误");
            string encryptNewPassword = newPassword.Sha256();

            // 在事务中修改密码
            await db.RunTransaction(async ctx =>
            {
                user.Password = encryptNewPassword;

                // 对 smtp 的密码先使用原密码解密，然后再用新密码加密
                var outboxes = await db.Outboxes.Where(x => x.UserId == userId).ToListAsync();
                foreach (var outbox in outboxes)
                {
                    // 原密钥
                    var smtpPassword = DecryptSmtpPassword(outbox.Password, passwordModel.OldSmtpPasswordSecretKeys);

                    // 计算新的密码
                    outbox.Password = EncryptSmtpPassword(smtpPassword, passwordModel.NewSmtpPasswordSecretKeys);
                }

                await db.SaveChangesAsync();
                return true;
            });

            return true;
        }

        /// <summary>
        /// 加密 smtp 密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string EncryptSmtpPassword(string password, SmtpPasswordSecretKeys secretKeys)
        {
            return password.AES(secretKeys.Key, secretKeys.Iv);
        }

        /// <summary>
        /// 解密 smtp 密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string DecryptSmtpPassword(string password, SmtpPasswordSecretKeys secretKeys)
        {
            return password.DeAES(secretKeys.Key, secretKeys.Iv);
        }
    }
}
