using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 后者的值会覆盖前者的值
    /// </summary>
    /// <param name="settings"></param>
    public class OrganizationSettingCache : BaseDBCache<SqlContext>
    {
        #region 接口实现
        public long OrganizationId => LongValue;
        public OrganizationSetting? Setting { get; private set; }

        public override void Dispose()
        {
            Setting = null;
            SetDirty(true);
        }

        /// <summary>
        /// key 为 organizationId
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override async Task Update(SqlContext db)
        {
            if (!NeedUpdate) return;
            SetDirty(false);

            Setting = await db.OrganizationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.OrganizationId == OrganizationId);
            if (Setting == null)
            {
                // 添加默认值
                Setting = new OrganizationSetting();
                db.Add(Setting);
                await db.SaveChangesAsync();
            }
        }
        #endregion

        #region 代理属性
        /// <summary>
        /// 发送邮件的最大批次大小
        /// </summary>
        public int MaxSendingBatchSize => Setting?.MaxSendingBatchSize ?? 0;

        /// <summary>
        /// 是否启用邮件跟踪器
        /// </summary>
        public bool EnableEmailTracker => Setting?.EnableEmailTracker ?? false;

        /// <summary>
        /// 当发件x次时，更换IP
        /// 为 0 时，表示不更换
        /// </summary>
        public int ChangeIpAfterEmailCount => Setting?.ChangeIpAfterEmailCount ?? 0;
        #endregion
    }
}
