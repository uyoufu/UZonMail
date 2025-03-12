using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.SignalRHubs.Extensions;
using UZonMail.Core.SignalRHubs.SendEmail;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 发件项后处理器
    /// 1. 判断是否可以重试
    /// 2. 更新发件状态到数据库
    /// 3. 发送通知
    /// </summary>
    public class EmailItemPostHandler : AbstractSendingHandler
    {
        protected override async Task HandleCore(SendingContext context)
        {
            // 发送发件进度
            if (context.EmailItem == null) return;

            // 根据状态发送进度信息
            var emailItem = context.EmailItem;
            // 判断发件项是否需要重试
            if (!emailItem.IsErrorOrSuccess())
            {
                if (emailItem.TriedCount >= emailItem.MaxRetryCount)
                {
                    // 说明已经达到了最大重试次数
                    emailItem.SetStatus(SendItemMetaStatus.Error, $"重试已达最大次数 {emailItem.MaxRetryCount}");
                }
            }

            // 标记结束
            emailItem.Done();

            if (!emailItem.IsErrorOrSuccess()) return;
            
            // 保存结果到数据库
            var sendingItem = await SaveSendingItemInfos(context);

            // 通知前端发件项状态变化
            var client = context.HubClient.GetUserClient(context.OutboxAddress.UserId);
            await client.SendingItemStatusChanged(new SendingItemStatusChangedArg(sendingItem));
        }

        /// <summary>
        /// 保存 SendItem 状态
        /// </summary>
        /// <returns></returns>
        private static async Task<SendingItem> SaveSendingItemInfos(SendingContext sendingContext)
        {
            var outbox = sendingContext.OutboxAddress;
            var emailItem = sendingContext.EmailItem;

            var db = sendingContext.SqlContext;

            var success = emailItem.Status.HasFlag(SendItemMetaStatus.Success);
            var message = emailItem.Message;

            // 更新 sendingItems 状态            
            var data = await db.SendingItems.FirstOrDefaultAsync(x => x.Id == emailItem.SendingItemId);
            // 更新数据
            data.FromEmail = outbox.Email;
            data.Subject = emailItem.Subject;
            data.Content = emailItem.HtmlBody;
            // 保存发送状态
            data.Status = success ? SendingItemStatus.Success : SendingItemStatus.Failed;
            data.SendResult = message;
            data.TriedCount = emailItem.TriedCount;
            data.SendDate = DateTime.Now;
            // 解析邮件 id
            data.ReceiptId = new ResultParser(message).GetReceiptId();

            // 更新 sendingItemInbox 状态
            await db.SendingItemInboxes.UpdateAsync(x => x.SendingItemId == emailItem.SendingItemId,
                x => x.SetProperty(y => y.FromEmail, outbox.Email)
                    .SetProperty(y => y.SendDate, DateTime.Now)
                );

            // 更新收件箱的最近收件日期
            await db.Inboxes.UpdateAsync(x => emailItem.Inboxes.Select(x => x.Id).Contains(x.Id),
                               x => x.SetProperty(y => y.LastBeDeliveredDate, DateTime.Now)
                                .SetProperty(y => y.LastSuccessDeliveryDate, DateTime.Now));

            await db.SaveChangesAsync();

            return data;
        }
    }
}
