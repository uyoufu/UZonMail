using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingPipeline(IServiceProvider provider)
        : ISendingPipeline,
            IScopedService<ISendingPipeline>
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SendingPipeline));

        private static readonly Type[] HandlerTypes =
        [
            typeof(EmailItemGetter),
            typeof(LocalEmailSendingHandler),
            typeof(PermanentOutboxFailureHandler),
            typeof(EmailItemUpdateHandler),
            typeof(GroupTaskUpdateHandler),
            typeof(OutboxesUpdateHandler),
            typeof(OutboxRetirementHandler),
            typeof(SmtpClientDisposer),
            typeof(OutboxSendingThrottleHandler)
        ];

        public async Task Handle(SendingContext context)
        {
            var chainHandlers = HandlerTypes
                .Select(provider.GetRequiredService)
                .Cast<ISendingHandler>()
                .ToList();

            if (chainHandlers.Count == 0)
            {
                _logger.Error("发件职责链为空，任务退出");
                context.HandleResults.Add(HandlerResult.Failed("发件职责链为空"));
                return;
            }

            foreach (var handler in chainHandlers)
            {
                var result = await handler.Execute(context);
                context.HandleResults.Add(result);
                if (result.ChainStatus is ChainStatus.BreakChain or ChainStatus.ShouldExitTask)
                    break;
            }
        }
    }
}
