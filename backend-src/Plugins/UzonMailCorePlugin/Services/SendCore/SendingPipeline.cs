using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore
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
            typeof(EmailItemUpdateHandler),
            typeof(GroupTaskUpdateHandler),
            typeof(OutboxesUpdateHandler),
            typeof(OutboxDisposer),
            typeof(SmtpClientDisposer),
            typeof(OutboxSendingThrottleHandler)
        ];

        public async Task Handle(SendingContext context)
        {
            var chainHandlers = HandlerTypes.Select(provider.GetRequiredService)
                .Cast<ISendingHandler>()
                .ToList();

            if (chainHandlers.Count == 0)
            {
                _logger.Error("发件职责链为空，任务退出");
                context.HandleResults.Add(HandlerResult.Failed("发件职责链为空"));
                return;
            }

            _ = chainHandlers.Aggregate((a, b) => a.SetNext(b));
            await chainHandlers.First().Handle(context);
        }
    }
}
