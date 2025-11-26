using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 用户信息缓存
    /// 一个 id 对应一个缓存实例
    /// </summary>
    public class UserInfoCache : BaseDBCache<SqlContext, long>
    {
        /// <summary>
        /// key 为 userId
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override async Task UpdateCore(SqlContext db)
        {
            // 获取用户信息
            var userInfo = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == UserId);
            if (userInfo != null)
            {
                DepartmentId = userInfo.DepartmentId;
                OrganizationId = userInfo.OrganizationId;
            }
        }

        public override void Dispose()
        {
            SetDirty();
        }

        public User? UserInfo { get; private set; }

        /// <summary>
        /// 用户 id
        /// </summary>
        public long UserId => Args;

        /// <summary>
        /// 用户部门 id
        /// </summary>
        public long DepartmentId { get; private set; }

        /// <summary>
        /// 用户组织 id
        /// </summary>
        public long OrganizationId { get; private set; }
    }
}
