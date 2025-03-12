using log4net;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Utils;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.MySql;
using UZonMail.DB.SQL;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件箱冷却器
    /// </summary>
    public class OutboxCooler(SendingThreadsManager sendingThreadsManager, SqlContext sqlContext) : AbstractSendingHandler
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(OutboxCooler));
        protected override async Task HandleCore(SendingContext context)
        {
            // 没有成功，不需要冷却
            if (!context.Status.HasFlag(ContextStatus.Success)) return;

            var outbox = context.OutboxAddress;
            if (outbox == null) return;

            // 被释放后，直接返回
            if (outbox.ShouldDispose) return;

            // 计算冷却时间
            UserInfoCache userInfo = await CacheManager.Global.GetCache<UserInfoCache, SqlContext>(sqlContext, outbox.UserId);
            var orgSetting = await CacheManager.Global.GetCache<OrganizationSettingCache, SqlContext>(sqlContext, userInfo.OrganizationId);
            int cooldownMilliseconds = orgSetting.Setting?.GetCooldownMilliseconds() ?? 0;
            if (cooldownMilliseconds <= 0) return;

            _logger.Info($"发件箱 {outbox.Email} 进入冷却状态，冷却时间 {cooldownMilliseconds} 毫秒");
            outbox.StartCooling(cooldownMilliseconds, sendingThreadsManager);
        }
    }
}
