using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.DB.SQL.Organization;

namespace UZonMail.DB.SQL.Permission
{
    /// <summary>
    /// 权限角色
    /// </summary>    
    [Index(nameof(Name), IsUnique = true)]
    public class Role : OrgId, IEntityTypeConfiguration<Role>
    {
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; } = "key";

        /// <summary>
        /// 角色的权限码数量
        /// </summary>
        public int PermissionCodesCount { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public List<long> PermissionCodeIds { get; set; } = [];

        #region 导航属性
        /// <summary>
        /// 权限码
        /// </summary>
        public List<PermissionCode> PermissionCodes { get; set; } = [];

        /// <summary>
        /// 用户权限
        /// </summary>
        public List<UserRoles>? UserRoles { get; set; } = [];
        #endregion

        /// <summary>
        /// 配置 PermissionCode 与 Role 多对多
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasMany(x => x.PermissionCodes).WithMany(x => x.Roles);
        }


        #region 静态属性
        /// <summary>
        /// 系统管理员角色名
        /// </summary>
        public static string OrganizationAdminRoleName => "OrganizationAdmin";
        #endregion
    }
}
