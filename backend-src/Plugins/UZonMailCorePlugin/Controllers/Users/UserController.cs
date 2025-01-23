using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Services.UserInfos;
using UZonMail.Core.Services.Files;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Utils.Database;
using UZonMail.Core.Controllers.Users.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.Core.Database.Validators;
using UZonMail.Core.Utils.Extensions;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Users
{
    /// <summary>
    /// 用户
    /// </summary>
    /// <remarks>
    /// 构造函数
    /// </remarks>
    /// <param name="userService"></param>
    public class UserController(SqlContext db, UserService userService, TokenService tokenService, FileStoreService fileStoreService) : ControllerBaseV1
    {
        /// <summary>
        /// UserId 是否已经被使用
        /// </summary>
        /// <returns></returns>
        [HttpGet("check-user-id")]
        public async Task<ResponseResult<bool>> IsUserIdInUse([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new KnownException("用户 ID 不能为空");
            var existUser = await userService.ExistUser(userId);
            if (existUser) throw new KnownException($"用户 ID: {userId} 已经存在");

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 新建用户
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("sign-up")]
        public async Task<ResponseResult<User>> SignUp([FromBody] User user)
        {
            // 验证用户名和密码
            var userVdResult = new UserIdAndPasswordValidator().Validate(user);
            if (!userVdResult.IsValid) return userVdResult.ToErrorResponse<User>();

            // 用户名重复检查
            var existUser = await userService.ExistUser(user.UserId);
            if (existUser) throw new KnownException($"用户 {user.UserId} 已经存在");

            // 返回新建用户
            var newUser = await userService.CreateUser(user.UserId, user.Password);
            // 清空密码
            newUser.Password = string.Empty;
            return newUser.ToSuccessResponse();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("sign-in"), AllowAnonymous]
        public async Task<ResponseResult<UserSignInResult>> SignIn([FromBody] User user)
        {
            // 验证用户
            // 验证用户名和密码
            var userVdResult = new UserIdAndPasswordValidator().Validate(user);
            if (!userVdResult.IsValid) return userVdResult.ToErrorResponse<UserSignInResult>();

            var loginResult = await userService.UserSignIn(user.UserId, user.Password);

            return loginResult.ToSuccessResponse();
        }

        /// <summary>
        /// 更新登陆用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("sign-in")]
        public async Task<ResponseResult<UserSignInResult>> UpdateSignIn()
        {
            var userId = tokenService.GetUserSqlId();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId) ?? throw new KnownException("用户不存在");
            var loginResult = await userService.UserSignIn(user);
            return loginResult.ToSuccessResponse();
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("user-info")]
        public async Task<ResponseResult<User>> GetUserInfo(string userId)
        {
            var user = await userService.GetUserInfo(userId);
            return user.ToSuccessResponse();
        }

        /// <summary>
        /// 退出登陆
        /// </summary>
        /// <returns></returns>
        [HttpPut("sign-out")]
        public async Task<ResponseResult<bool>> UserSignOut()
        {
            // 从 token 中获取当前用户信息

            // 清除用户的 signR 连接

            // 返回退出成功消息
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取用户的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetUsersCount([FromQuery] string filter)
        {
            var count = await userService.GetFilteredUsersCount(filter);
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取过滤后的数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<User>>> GetUsersData([FromQuery] string filter, [FromBody] Pagination pagination)
        {
            var users = await userService.GetFilteredUsersData(filter, pagination);
            return users.ToSuccessResponse();
        }

        /// <summary>
        /// 获取所有的用户
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ResponseResult<List<User>>> GetAllUsers()
        {
            var users = await db.Users.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync();
            return users.ToSuccessResponse();
        }

        /// <summary>
        /// 获取默认的用户密码
        /// 只有超管才可以操作
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("default-password")]
        public async Task<ResponseResult<string>> GetDefaultPassword()
        {
            string pwd = userService.GetUserDefaultPassword();
            return pwd.ToSuccessResponse();
        }

        /// <summary>
        /// 重置用户密码
        /// 只有超管才可以操作
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("reset-password")]
        public async Task<ResponseResult<bool>> ResetUserPassword([FromBody] User user)
        {
            // 查找用户
            bool result = await userService.ResetUserPassword(user.UserId);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        [HttpPut("password")]
        public async Task<ResponseResult<bool>> ChangeUserPassword([FromBody] ChangePasswordModel passwordModel)
        {
            // 获取当前用户
            var userId = tokenService.GetUserSqlId();
            // 验证原密码是否正确
            bool result = await userService.ChangeUserPassword(userId, passwordModel);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPut("avatar")]
        public async Task<ResponseResult<string>> UpdateUserAvatar(IFormFile file)
        {
            if (file == null) throw new KnownException("文件不能为空");

            var userId = tokenService.GetUserSqlId();
            var (fullPath, relativePath) = fileStoreService.GenerateStaticFilePath(userId.ToString(), "avatar", DateTime.Now.ToTimestamp() + "_" + file.FileName);

            // 清除原来的头像文件
            string baseDir = Path.GetDirectoryName(fullPath) ?? throw new KnownException("文件路径错误");
            // 删除 baseDir 下的所有文件
            foreach (var filePath in Directory.GetFiles(baseDir))
            {
                System.IO.File.Delete(filePath);
            }

            // 保存文件
            using Stream saveFile = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(saveFile);

            // 将头像更新到用户信息中
            await db.Users.UpdateAsync(x => x.Id == userId, x => x.SetProperty(y => y.Avatar, relativePath));
            await db.SaveChangesAsync();

            return relativePath.ToSuccessResponse();
        }

        /// <summary>
        /// 修改用户类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId:long}/type")]
        public async Task<ResponseResult<bool>> ChangeUserType(long userId, UserType userType)
        {
            var result = await userService.UpdateUserType(userId, userType);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId:long}/status")]
        public async Task<ResponseResult<bool>> ChangeUserStatus(long userId, UserStatus status)
        {
            await db.Users.UpdateAsync(x => x.Id == userId, x => x.SetProperty(p => p.Status, status));
            return true.ToSuccessResponse();
        }
    }
}
