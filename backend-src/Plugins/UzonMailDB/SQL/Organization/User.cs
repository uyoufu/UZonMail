using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Permission;

namespace UZonMail.DB.SQL.Organization
{
    /// <summary>
    /// 用户上下文
    /// </summary>
    public class User : SqlId
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// 当前所在组织 Id
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        /// 当前所在部门 Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// signalR 连接 id
        /// </summary>
        public string? ConnectionId { get; set; }

        /// <summary>
        /// 是否是超级管理员
        /// 超级管理员拥有所有权限
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// 系统用户
        /// 系统内使用的用户
        /// </summary>
        public bool IsSystsemUser { get; set; }

        /// <summary>
        /// 用户角色
        /// 导航属性
        /// </summary>
        public UserRoles? UserRoles { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// 由谁创建
        /// </summary>
        public long CreateBy { get; set; }

        #region 静态变量
        public static string SystemUserId => "system_uzon";
        #endregion
    }
}
