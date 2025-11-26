using System.Collections;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Templates;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 用户模板缓存
    /// 一个 id 对应多个缓存实例
    /// </summary>
    public class UserTemplatesCache : BaseDBCache<SqlContext, long>, IEnumerable<EmailTemplate>
    {
        private List<EmailTemplate> _templates = [];

        public long UserId => Args;

        protected override async Task UpdateCore(SqlContext db)
        {
            var userInfo = await DBCacheManager.Global.GetCache<UserInfoCache, SqlContext>(
                db,
                UserId
            );

            // 按用户缓存代理
            _templates = await db
                .EmailTemplates.AsNoTracking()
                .Where(x =>
                    x.UserId == UserId
                    || x.ShareToUsers.Select(x => x.Id).Contains(UserId)
                    || x.ShareToOrganizations.Select(x => x.Id).Contains(userInfo.OrganizationId)
                )
                .ToListAsync();
        }

        public override void Dispose()
        {
            _templates.Clear();
            SetDirty();
        }

        public IEnumerator<EmailTemplate> GetEnumerator()
        {
            return _templates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _templates.GetEnumerator();
        }
    }
}
