using log4net;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    public class SenderAccountsUpdateHandler(
        UserGroupTasksPools userGroupTasksPools,
        SenderAccountsManager senderAccountManager,
        AppSettingsManager settingsService
    ) : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(SenderAccountsUpdateHandler)
        );

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 移除发件箱：
            // 1. 发件箱错误
            // 2. 发件数量达到上限
            // 3. 无件可发。特定发件箱,没有件可发; 非特定发件箱，发件组已清除

            // 若因为发件箱移除，需要同时移除发件组时，还要下发发件组状态变更通知

            var senderAccount = context.SenderAccountAddress;
            var currentAttempt = context.CurrentAttempt;

            if (senderAccount == null)
                return HandlerResult.Skiped();

            if (currentAttempt == null)
            {
                // 没有发件项时，可会存在所有发件正在发送中的情况，因此 senderAccount 不能立马释放, 需要进行判断
                if (!MatchEmailItem(senderAccount))
                {
                    var message = "未匹配到可发邮件,主动释放";
                    _logger.Info(message);
                    // 移除
                    senderAccountManager.RemoveSenderAccount(senderAccount, message);
                }

                return HandlerResult.Skiped();
            }

            // 增加发件数量
            if (context.SendAttemptDecision is { IsTerminal: true })
            {
                // 判断是否达到了最大的发件数限制
                senderAccount.IncreaseSentCount();
                await context.SqlContext.SenderAccounts.UpdateAsync(
                    x => x.Id == senderAccount.Id,
                    x =>
                        x.SetProperty(y => y.SentTotalToday, senderAccount.SentTotalToday)
                            .SetProperty(y => y.SentCountDateUtc, senderAccount.SentCountDateUtc)
                );

                // 从发件箱中移除特定发件项
                senderAccount.RemoveSepecificSendingItem(
                    currentAttempt.Descriptor.SendingGroupId,
                    currentAttempt.Descriptor.Id
                );
            }

            // 检查发件箱发件数量是否超限
            // 若超限，则标记为需要释放
            if (await CheckSenderAccountSentCountLimit(context.SqlContext, senderAccount))
            {
                var utcNow = DateTimeOffset.UtcNow;
                foreach (var sendingGroupId in senderAccount.GetSendingGroupIds())
                {
                    if (
                        !senderAccountManager.AreAllSenderAccountsQuotaBlocked(
                            sendingGroupId,
                            utcNow
                        )
                    )
                        continue;
                    var hasPendingItems = await context.SqlContext.SendingItems.AnyAsync(item =>
                        item.SendingGroupId == sendingGroupId
                        && (
                            item.Status == SendingItemStatus.Pending
                            || item.Status == SendingItemStatus.Sending
                        )
                    );
                    if (!hasPendingItems)
                        continue;
                    await context.SqlContext.SendingGroups.UpdateAsync(
                        group => group.Id == sendingGroupId,
                        setters =>
                            setters
                                .SetProperty(
                                    group => group.Status,
                                    SendingGroupStatus.WaitingForQuotaReset
                                )
                                .SetProperty(group => group.StatusReason, "所有可用发件箱均达到当日发送额度")
                                .SetProperty(
                                    group => group.ResumeAtUtc,
                                    senderAccount.QuotaBlockedUntilUtc.UtcDateTime
                                )
                    );
                }
            }

            return HandlerResult.Success();
        }

        /// <summary>
        /// 判断发件箱是否还有邮件需要发
        /// </summary>
        /// <param name="senderAccount"></param>
        /// <returns></returns>
        private bool MatchEmailItem(SenderEmailAddress senderAccount)
        {
            if (!userGroupTasksPools.TryGetValue(senderAccount.UserId, out var groupTasks))
                return false;
            return groupTasks.MatchEmailItem(senderAccount);
        }

        /// <summary>
        /// 检查发件箱的发件数量限制
        /// </summary>
        private async Task<bool> CheckSenderAccountSentCountLimit(
            SqlContext sqlContext,
            SenderEmailAddress senderAccount
        )
        {
            var orgSetting = await settingsService.GetSetting<SendingSetting>(
                sqlContext,
                senderAccount.UserId
            );

            // 本身有限制时，若已经达到发送上限，则不再发送
            var overflowLimit = false;
            if (senderAccount.MaxSendCountPerDay > 0)
            {
                if (senderAccount.SentTotalToday >= senderAccount.MaxSendCountPerDay)
                {
                    overflowLimit = true;
                }
            }
            // 本身没限制，使用系统的限制
            else if (
                orgSetting.MaxSendCountPerEmailDay > 0
                && senderAccount.SentTotalToday >= orgSetting.MaxSendCountPerEmailDay
            )
            {
                overflowLimit = true;
            }

            if (overflowLimit)
            {
                var message =
                    $"发件箱 {senderAccount.Email} 已达当日最大发件量: {senderAccount.SentTotalToday}";
                _logger.Warn(message);
                senderAccount.ScheduleDailyQuotaReset(DateTimeOffset.UtcNow);
            }
            return overflowLimit;
        }
    }
}
