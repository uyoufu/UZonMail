using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发送项获取器
    /// </summary>
    public class EmailItemGetter(GroupTasksManager groupTasksManager) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailItemGetter));

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 如果前面失败了，直接报错
            if (context.IsFailed())
                return HandlerResult.Failed();

            var outbox = context.OutboxAddress;
            if (outbox == null)
                return HandlerResult.Failed("发件箱信息为空，无法申请发件项");

            _logger.Debug($"发件箱 {outbox.Email} 开始申请发件项");

            // 从等待列表中获取一个发送项
            var emailItem = await groupTasksManager.GetEmailItem(context);

            // 修改状态
            emailItem?.SetStatus(SendItemMetaStatus.Working);
            // 保存到 context 中
            context.EmailItem = emailItem;

            // 标记失败
            if (emailItem == null)
            {
                return HandlerResult.Failed("线程申请发件项失败，可能没有可用的发件项");
            }
            else
            {
                _logger.Info($"线程申请发件项成功，收件箱：{emailItem.Inboxes.Select(x => x.Email)}");
            }

            return HandlerResult.Success();
        }
    }
}
