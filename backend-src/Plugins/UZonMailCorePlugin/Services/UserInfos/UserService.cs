using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Uamazing.Utils.Web.Token;
using UZonMail.CorePlugin.Config;
using UZonMail.CorePlugin.Controllers.Users.Model;
using UZonMail.CorePlugin.Services.Config;
using UZonMail.CorePlugin.Services.Encrypt;
using UZonMail.CorePlugin.Services.Permission;
using UZonMail.CorePlugin.Services.Plugin;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.DB.SQL.Core.Permission;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.Service;
using UZonMail.Utils.Web.Token;

namespace UZonMail.CorePlugin.Services.UserInfos
{
    /// <summary>
    /// 只在请求生命周期内有效的服务
    /// </summary>
    public class UserService(
        IServiceProvider serviceProvider,
        SqlContext db,
        IOptions<AppConfig> appConfig,
        PermissionService permission,
        PluginService pluginService,
        TokenService tokenService,
        DebugConfig debugConfig,
        EncryptService encryptService
    ) : IScopedService
    {
        /// <summary>
        /// 判断用户是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> ExistUser(string userId)
        {
            return await db.Users.FirstOrDefaultAsync(x => x.UserId.ToLower() == userId.ToLower())
                != null;
        }

        /// <summary>
        /// 创建用户的默认部门和组织
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parentOrganizationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<Tuple<Department, Department>> CreateUserHomeDepartments(
            SqlContext ctx,
            long parentOrganizationId,
            string userId
        )
        {
            var orgName = $"Org-{userId}";
            var departmentName = $"Department-{userId}";

            var organization = await ctx.Departments.FirstOrDefaultAsync(x =>
                x.ParentId == parentOrganizationId
                && x.Name == orgName
                && x.Type == DepartmentType.Organization
            );
            if (organization == null)
            {
                var parentOrganization = await ctx.Departments.FirstOrDefaultAsync(x =>
                    x.Id == parentOrganizationId
                );
                // 在当前用户组织下创建新的组织
                organization = new Department()
                {
                    ParentId = parentOrganizationId,
                    Name = orgName,
                    Description = $"{userId}的组织",
                    Type = DepartmentType.Organization,
                    FullPath = $"{parentOrganization.FullPath}/{orgName}"
                };
                ctx.Departments.Add(organization);
                await ctx.SaveChangesAsync();
            }

            var department = await ctx.Departments.FirstOrDefaultAsync(x =>
                x.ParentId == organization.Id
                && x.Name == departmentName
                && x.Type == DepartmentType.Department
            );
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
                ctx.Departments.Add(department);
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
            var adminUserRole = await ctx
                .UserRole.Include(x => x.User)
                .Where(x => x.User.IsSuperAdmin)
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .FirstOrDefaultAsync();
            if (adminUserRole == null)
                return;

            // 获取超管角色
            var orgRole = adminUserRole
                .Roles.Where(x =>
                    x.PermissionCodes.Any(y => y.Code == PermissionCode.OrganizationPermissionCode)
                )
                .FirstOrDefault();
            if (orgRole == null)
                return;

            // 为用户添加组织管理员角色
            var userRole = await ctx
                .UserRole.Where(x => x.UserId == userId)
                .Include(x => x.Roles)
                .FirstOrDefaultAsync();
            if (userRole == null)
            {
                userRole = new UserRoles() { UserId = userId, };
                ctx.UserRole.Add(userRole);
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
            var userRole = await ctx
                .UserRole.Where(x => x.UserId == userId)
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .FirstOrDefaultAsync();
            var orgRoles = userRole
                .Roles.Where(x =>
                    x.PermissionCodes.Any(y => y.Code == PermissionCode.OrganizationPermissionCode)
                )
                .ToList();
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
        /// <param name="password">密码是前端传递的密码</param>
        /// <returns></returns>
        public async Task<User> CreateUser(string userId, string password)
        {
            var organizationId = tokenService.GetOrganizationId();
            return await db.RunTransaction(async ctx =>
            {
                var (organization, department) = await CreateUserHomeDepartments(
                    ctx,
                    organizationId,
                    userId
                );
                var userSalt = User.NewSalt();
                var user = new User()
                {
                    OrganizationId = organization.Id,
                    DepartmentId = department.Id,
                    UserId = userId,
                    Password = encryptService.HashPassword(password, userSalt),
                    Salt = userSalt,
                    Type = UserType.Independent,
                    CreateBy = tokenService.GetUserSqlId(),
                };
                ctx.Users.Add(user);
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
            var user =
                await db.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new KnownException("用户不存在");
            if (user.Type == type)
                return true;

            // 只能修改由自己创建的用户
            var handlerId = tokenService.GetUserSqlId();
            if (user.CreateBy != handlerId)
                throw new KnownException("只能操作由自己创建的用户");

            user.Type = type;
            var organizationId = tokenService.GetOrganizationId();

            if (type == UserType.SubUser)
            {
                // 放到自己组织下
                var department =
                    await db.Departments.FirstOrDefaultAsync(x =>
                        x.ParentId == organizationId && x.Type == DepartmentType.Department
                    ) ?? throw new KnownException("部门不存在");
                user.OrganizationId = organizationId;
                user.DepartmentId = department.Id;

                // 移除组织管理员权限
                await RemoveOrganizationAdminRole(db, user.Id);
            }
            else
            {
                // 放到其原来的组织下
                var (organization, department) = await CreateUserHomeDepartments(
                    db,
                    organizationId,
                    user.UserId
                );
                user.OrganizationId = organization.Id;
                user.DepartmentId = department.Id;

                // 为用户分配组织管理员
                await AssignOrganizationAdminRole(db, user.Id);
            }
            await db.SaveChangesAsync();

            // 更新用户的组织设置和和退订设置
            DBCacheManager.Global.SetCacheDirty<UserInfoCache>(user.Id);

            return true;
        }

        /// <summary>
        /// 通过用户 ID 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserInfo(int userId)
        {
            var user =
                await db.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new KnownException(userId + "用户不存在");
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
            var user =
                await db.Users.FirstOrDefaultAsync(x => x.UserId == userId)
                ?? throw new KnownException(userId + "用户不存在");
            // 将密码置空
            user.Password = string.Empty;
            return user;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password">密码为原值</param>
        /// <returns></returns>
        /// <exception cref="KnownException"></exception>
        public async Task<UserSignInResult> UserSignIn(string userId, string password)
        {
            var errMsg = "用户名或密码错误";
            var userInfo =
                await db.Users.FirstOrDefaultAsync(x => x.UserId == userId)
                ?? throw new KnownException(errMsg);

            var encryptedPassword = encryptService.HashPassword(password, userInfo.Salt);

            if (userInfo.Password != encryptedPassword)
                throw new KnownException(errMsg);

            return await CreateSignSuccessResult(userInfo);
        }

        public async Task<UserSignInResult> CreateSignSuccessResult(User user)
        {
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
        /// <param name="expireMilliseconds">等于 0 时表示使用默认值</param>
        /// <param name="extraClaims">额外传递的 claims</param>
        /// <returns></returns>
        public async Task<string> GenerateToken(
            User userInfo,
            long expireMilliseconds = 0,
            IEnumerable<Claim>? extraClaims = null
        )
        {
            var expireDate =
                expireMilliseconds > 0
                    ? DateTime.UtcNow.AddMilliseconds(expireMilliseconds)
                    : DateTime.MaxValue;
            return await GenerateToken(userInfo, expireDate, extraClaims);
        }

        public async Task<string> GenerateToken(
            User userInfo,
            DateTime expireDate,
            IEnumerable<Claim>? extraClaims = null
        )
        {
            var tokenClaimBuilders = serviceProvider.GetServices<ITokenClaimBuilder>();
            List<Claim> claims = [];
            if (extraClaims != null)
            {
                claims.AddRange(extraClaims);
            }

            foreach (var claimBuilder in tokenClaimBuilders)
            {
                var claimsTemp = await claimBuilder.Build(userInfo);
                claims.AddRange(claimsTemp);
            }

            var tokenParams = appConfig.Value.TokenParams.Clone();
            // 若
            if (expireDate > DateTime.UtcNow)
                tokenParams.ExpireDate = expireDate;
            else if (tokenParams.Expire > 0)
                tokenParams.ExpireDate = DateTime.UtcNow.AddMilliseconds(tokenParams.Expire);
            else
                tokenParams.ExpireDate = DateTime.MinValue;

            string token = JWTToken.CreateToken(tokenParams, claims);
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
            return db
                .Users.Where(x => !x.IsDeleted && !x.IsHidden && !x.IsSuperAdmin)
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
            var user =
                await db.Users.FirstOrDefaultAsync(x => x.UserId == userId)
                ?? throw new KnownException("用户不存在");
            var newSalt = User.NewSalt();
            var newPassword = encryptService.HashPassword(
                appConfig.Value.User.DefaultPassword.Sha256(),
                newSalt
            );

            user.Password = newPassword;
            user.Salt = newSalt;

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
            if (debugConfig.IsDemo)
                throw new KnownException("演示环境不允许修改密码");

            var oldPassword = passwordModel.OldPassword;
            var newPassword = passwordModel.NewPassword;

            if (
                userId <= 0
                || string.IsNullOrEmpty(oldPassword)
                || string.IsNullOrEmpty(newPassword)
            )
            {
                throw new KnownException("新旧密码不能为空");
            }

            var errorMsg = "用户名或密码错误";

            var userInfo =
                await db.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new KnownException(errorMsg);

            // 查找用户
            string oldEncryptedPassword = encryptService.HashPassword(oldPassword, userInfo.Salt);
            if (oldEncryptedPassword != userInfo.Password)
            {
                throw new KnownException(errorMsg);
            }

            // 每次修改密码，都生成新的 salt
            var newSalt = User.NewSalt();
            string newEncryptedPassword = encryptService.HashPassword(newPassword, newSalt);

            userInfo.Password = newEncryptedPassword;
            userInfo.Salt = newSalt;
            await db.SaveChangesAsync();

            return true;
        }
    }
}
