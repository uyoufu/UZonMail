using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.DB.SQL.Permission
{
    /// <summary>
    /// 用户角色
    /// 为了不影响用户表，将用户的角色单独抽出来
    /// </summary>
    public class UserRoles : OrgId
    {
        /// <summary>
        /// 用户的 Id
        /// </summary>
        public long UserId { get; set; }

        #region 导航属性
        public User User { get; set; }

        /// <summary>
        /// 与 Role 是多对多关系
        /// </summary>
        public List<Role> Roles { get; set; } = [];
        #endregion
    }
}
