using log4net;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Outboxes;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 获取发件箱
    /// </summary>
    /// <param name="container"></param>
    [Obsolete("已弃用，未来将移除")]
    public class OutboxGetter(OutboxesPoolList container) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OutboxGetter));
        protected override async Task HandleCore(SendingContext context)
        {
            _logger.Debug("线程开始申请发件箱");

            // 获取发件箱
            var address = container.GetOutbox();
            // 保存到 context 中
            context.SetOutbox(address);

            // 如果获取失败，则停止线程
            if (address == null)
            {
                context.Status |= ContextStatus.Fail;
                context.Status |= ContextStatus.ShouldExitTask;
            }
            else
            {
                _logger.Debug($"线程申请发件箱成功：{address!.Email}");
            }
            
            return;
        }
    }
}
