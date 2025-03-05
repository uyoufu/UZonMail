using Microsoft.EntityFrameworkCore;
using System.Collections;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.DB.SQL.Settings;
using UZonMail.DB.SQL.Templates;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 用户代理缓存
    /// 每隔一段时间要自动释放内存
    /// key 值为 userId
    /// </summary>
    public class UserProxiesCache : BaseCache, IEnumerable<Proxy>
    {
        public long UserId => LongValue;
        private List<Proxy> _proxies { get; set; } = [];

        public int Count => _proxies.Count;

        public override async Task Update(SqlContext db)
        {
            if (!NeedUpdate) return;
            SetDirty(false);

            var userInfo = await CacheManager.Global.GetCache<UserInfoCache>(db, UserId);
            // 按用户缓存代理
            _proxies = await db.Proxies
                .AsNoTracking()
                .Where(x => x.OrganizationId == userInfo.OrganizationId)
                .ToListAsync();
        }

        public override void Dispose()
        {
            _proxies.Clear();
            SetDirty();
        }

        #region 支持迭代
        public IEnumerator<Proxy> GetEnumerator()
        {
            return _proxies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
