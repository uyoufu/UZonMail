using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.Core.Permission
{
    /// <summary>
    /// 权限码
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class PermissionCode : SqlId
    {
        /// <summary>
        /// 权限码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否是负权限
        /// 负权限表示拥有此权限的角色不能访问此功能
        /// </summary>
        public bool IsNegative { get; set; }

        /// <summary>
        /// 关联的角色
        /// </summary>
        public List<Role> Roles { get; set; }


        #region 静态变量
        public static string OrganizationPermissionCode => "organizationAdmin";
        #endregion
    }
}
