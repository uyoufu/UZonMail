using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class OutboxSendingSpeedController(
        SqlContext sqlContext,
        AppSettingsManager settingsService
    ) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(OutboxSendingSpeedController)
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

            _logger.Info($"发件箱 {outbox.Email} 进入冷却状态，冷却时间 {cooldownMilliseconds} 毫秒");
            await Task.Delay(cooldownMilliseconds);
            _logger.Info($"发件箱 {outbox.Email} 冷却结束");
            return HandlerResult.Success();
        }
    }
}
