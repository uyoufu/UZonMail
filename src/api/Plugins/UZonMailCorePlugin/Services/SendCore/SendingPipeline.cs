using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingPipeline : ISendingPipeline, IScopedService<ISendingPipeline>
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SendingPipeline));
        private readonly IServiceProvider? _provider;
        private readonly IReadOnlyList<ISendingHandler>? _configuredHandlers;

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

        /// <summary>
        /// 使用服务容器按固定业务顺序解析发送责任链。
        /// </summary>
        public SendingPipeline(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 使用已排序的处理器构造责任链，供离线验证和组合场景复用。
        /// </summary>
        internal SendingPipeline(IReadOnlyList<ISendingHandler> configuredHandlers)
        {
            _configuredHandlers = configuredHandlers;
        }

        public async Task Handle(SendingContext context)
        {
            var chainHandlers =
                _configuredHandlers
                ?? HandlerTypes
                    .Select(_provider!.GetRequiredService)
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
