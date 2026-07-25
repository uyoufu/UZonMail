using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class OutboxSendingThrottleHandler(
        SqlContext sqlContext,
        AppSettingsManager settingsService
    ) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(OutboxSendingThrottleHandler)
        );

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 没有成功，不需要冷却
            if (context.IsFailed())
                return HandlerResult.Skiped();
            ;

            var outbox = context.OutboxAddress;
            if (outbox == null)
                return HandlerResult.Skiped();

            // 被释放后，直接返回
            if (outbox.ShouldDispose)
                return HandlerResult.Success();

            // 计算冷却时间
            var orgSetting = await settingsService.GetSetting<SendingSetting>(
                sqlContext,
                outbox.UserId
            );

            int cooldownMilliseconds = orgSetting.GetCooldownMilliseconds();
            if (cooldownMilliseconds <= 0)
                return HandlerResult.Skiped();

            // 仅记录下一次可调度时间；若在此等待，20 秒冷却会长期占住全局工作槽。
            outbox.ScheduleCooldown(
                TimeSpan.FromMilliseconds(cooldownMilliseconds),
                DateTimeOffset.UtcNow
            );
            _logger.Info($"发件箱 {outbox.Email} 进入冷却状态，冷却时间 {cooldownMilliseconds} 毫秒");
            return HandlerResult.Success();
        }
    }
}
