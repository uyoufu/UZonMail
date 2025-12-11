using log4net;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class OutboxesUpdateHandler(
        UserGroupTasksPools userGroupTasksPools,
        OutboxesManager outboxManager,
        AppSettingsManager settingsService
    ) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OutboxesUpdateHandler));

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 移除发件箱：
            // 1. 发件箱错误
            // 2. 发件数量达到上限
            // 3. 无件可发。特定发件箱,没有件可发; 非特定发件箱，发件组已清除

            // 若因为发件箱移除，需要同时移除发件组时，还要下发发件组状态变更通知

            var outbox = context.OutboxAddress;
            var emailItem = context.EmailItem;

            if (outbox == null)
                return HandlerResult.Skiped();

            if (emailItem == null)
            {
                // 没有发件项时，可会存在所有发件正在发送中的情况，因此 outbox 不能立马释放, 需要进行判断
                if (!MatchEmailItem(outbox))
                {
                    var message = "未匹配到可发邮件,主动释放";
                    _logger.Info(message);
                    // 移除
                    outboxManager.RemoveOutbox(outbox, message);
                }

                return HandlerResult.Skiped();
            }

            // 增加发件数量
            if (emailItem.IsErrorOrSuccess())
            {
                // 判断是否达到了最大的发件数限制
                outbox.IncreaseSentCount();

                // 从发件箱中移除特定发件项
                outbox.RemoveSepecificSendingItem(
                    emailItem.SendingItem.SendingGroupId,
                    emailItem.SendingItemId
                );
            }

            // 检查发件箱发件数量是否超限
            // 若超限，则标记为需要释放
            await CheckOutboxSentCountLimit(context.SqlContext, outbox);

            return HandlerResult.Success();
        }

        /// <summary>
        /// 判断发件箱是否还有邮件需要发
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        private bool MatchEmailItem(OutboxEmailAddress outbox)
        {
            if (!userGroupTasksPools.TryGetValue(outbox.UserId, out var groupTasks))
                return false;
            return groupTasks.MatchEmailItem(outbox);
        }

        /// <summary>
        /// 检查发件箱的发件数量限制
        /// </summary>
        private async Task CheckOutboxSentCountLimit(
            SqlContext sqlContext,
            OutboxEmailAddress outbox
        )
        {
            var orgSetting = await settingsService.GetSetting<SendingSetting>(
                sqlContext,
                outbox.UserId
            );

            // 本身有限制时，若已经达到发送上限，则不再发送
            var overflowLimit = false;
            if (outbox.MaxSendCountPerDay > 0)
            {
                if (outbox.SentTotalToday > outbox.MaxSendCountPerDay)
                {
                    overflowLimit = true;
                }
            }
            // 本身没限制，使用系统的限制
            else if (
                orgSetting.MaxSendCountPerEmailDay > 0
                && outbox.SentTotalToday >= orgSetting.MaxSendCountPerEmailDay
            )
            {
                overflowLimit = true;
            }

            if (overflowLimit)
            {
                var message = $"发件箱 {outbox.Email} 已达当日最大发件量: ${outbox.SentTotalToday}";
                _logger.Warn(message);
                outbox.MarkShouldDispose(message);
            }
        }
    }
}
