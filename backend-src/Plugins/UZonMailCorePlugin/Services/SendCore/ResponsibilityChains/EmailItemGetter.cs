using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发送项获取器
    /// </summary>
    public class EmailItemGetter(GroupTasksList groupTasksList) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailItemGetter));

        protected override async Task HandleCore(SendingContext context)
        {
            _logger.Debug("线程开始申请发件项");

            // 如果前面失败了，这一步就不执行
            if (context.Status.HasFlag(ContextStatus.Fail))
                return;

            // 从等待列表中获取一个发送项
            var emailItem = await groupTasksList.GetEmailItem(context);

            // 修改状态
            emailItem?.SetStatus(SendItemMetaStatus.Working);
            // 保存到 context 中
            context.EmailItem = emailItem;

            // 标记失败
            if (emailItem == null)
            {
                context.Status |= ContextStatus.Fail;
            }

            _logger.Debug("线程申请发件项成功");
        }
    }
}
