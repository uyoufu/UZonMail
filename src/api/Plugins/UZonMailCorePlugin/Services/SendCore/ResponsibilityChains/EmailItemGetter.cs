using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.WaitList;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
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

            var senderAccount = context.SenderAccountAddress;
            if (senderAccount == null)
                return HandlerResult.Failed("发件箱信息为空，无法申请发件项");

            _logger.Debug($"发件箱 {senderAccount.Email} 开始申请发件项");

            // 从等待列表中获取一个发送项
            var currentAttempt = await groupTasksManager.GetEmailItem(context);
            context.CurrentAttempt = currentAttempt;

            // 标记失败
            if (currentAttempt == null)
            {
                return HandlerResult.Failed("线程申请发件项失败，可能没有可用的发件项");
            }
            else
            {
                _logger.Info(
                    $"线程申请发件项成功，收件箱：{string.Join(",", currentAttempt.PreparedItem.Recipients.Select(x => x.Email))}"
                );
            }

            return HandlerResult.Success();
        }
    }
}
