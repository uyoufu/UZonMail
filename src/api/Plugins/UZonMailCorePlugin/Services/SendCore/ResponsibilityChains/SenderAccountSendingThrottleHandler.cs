using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class SenderAccountSendingThrottleHandler(
        SqlContext sqlContext,
        AppSettingsManager settingsService
    ) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(SenderAccountSendingThrottleHandler)
        );

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 没有成功，不需要冷却
            if (context.IsFailed())
                return HandlerResult.Skiped();
            ;

            var senderAccount = context.SenderAccountAddress;
            if (senderAccount == null)
                return HandlerResult.Skiped();

            // 被释放后，直接返回
            if (senderAccount.ShouldDispose)
                return HandlerResult.Success();

            // 计算冷却时间
            var orgSetting = await settingsService.GetSetting<SendingSetting>(
                sqlContext,
                senderAccount.UserId
            );

            int cooldownMilliseconds = orgSetting.GetCooldownMilliseconds();
            if (cooldownMilliseconds <= 0)
                return HandlerResult.Skiped();

            // 仅记录下一次可调度时间；若在此等待，20 秒冷却会长期占住全局工作槽。
            senderAccount.ScheduleCooldown(
                TimeSpan.FromMilliseconds(cooldownMilliseconds),
                DateTimeOffset.UtcNow
            );
            _logger.Info($"发件箱 {senderAccount.Email} 进入冷却状态，冷却时间 {cooldownMilliseconds} 毫秒");
            return HandlerResult.Success();
        }
    }
}
