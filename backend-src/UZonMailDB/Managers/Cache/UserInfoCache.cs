using Microsoft.EntityFrameworkCore;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 用户信息缓存
    /// </summary>
    public class UserInfoCache : BaseDBCache<SqlContext>
    {
        /// <summary>
        /// key 为 userId
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public override async Task Update(SqlContext db)
        {
            if (!NeedUpdate) return;
            SetDirty();

            // 获取用户信息
            UserInfo = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == UserId);
        }

        public override void Dispose()
        {
            UserInfo = null;
            SetDirty();
        }

        public User? UserInfo { get; private set; }

        /// <summary>
        /// 用户 id
        /// </summary>
        public long UserId => LongValue;

        /// <summary>
        /// 用户部门 id
        /// </summary>
        public long DepartmentId => UserInfo.DepartmentId;

        /// <summary>
        /// 用户组织 id
        /// </summary>
        public long OrganizationId => UserInfo.OrganizationId;
    }
}
