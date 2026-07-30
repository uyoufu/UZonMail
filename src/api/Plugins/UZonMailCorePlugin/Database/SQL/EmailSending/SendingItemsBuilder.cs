using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Json;

namespace UzonMail.CorePlugin.Database.SQL.EmailSending
{
    /// <summary>
    /// 用于生成发送项
    /// 请确保数据经过验证
    /// </summary>
    /// <param name="db">当前事务的数据库上下文。</param>
    /// <param name="group">正在创建的发件组。</param>
    /// <param name="maxSendingBatchSize">合并发件的最大收件人数。</param>
    /// <param name="allowDuplicateSending">是否保留 Excel 中的重复收件行。</param>
    /// <param name="excelAttachments">已由创建服务校验过的 Excel 附件映射。</param>
    /// <param name="organizationId">当前发件用户所属组织，用于过滤组织内无效收件箱。</param>
    public class SendingItemsBuilder(
        SqlContext db,
        SendingGroup group,
        int maxSendingBatchSize,
        bool allowDuplicateSending,
        IReadOnlyDictionary<string, FileUsage> excelAttachments,
        long organizationId
    )
    {
        /// <summary>
        /// 批量发件时的大小
        /// </summary>
        private readonly int _batchSize = maxSendingBatchSize;

        /// <summary>
        /// 生成并保存
        /// </summary>
        /// <returns></returns>
        public async Task<List<SendingItem>> GenerateAndSave()
        {
            // 获取收件箱
            List<SendingRecipient> recipients = await GetAllRecipients();
            // 更新发件箱总数
            group.InboxesCount = recipients.Count;

            // allInboxes 有的是从数据中解析得到的，需要获取其 id
            var inboxesWithoutId = recipients.Select(x => x.Inbox).Where(x => x.Id == 0).ToList();
            // 获取当前用户下的发件箱
            var inboxesEmails = inboxesWithoutId.Select(x => x.Email).Distinct().ToList();
            var inboxes = await db
                .Inboxes.AsNoTracking()
                .Where(x => x.UserId == group.UserId && inboxesEmails.Contains(x.Email))
                .ToListAsync();
            inboxesWithoutId.ForEach(x =>
            {
                var existInbox = inboxes.FirstOrDefault(i =>
                    string.Equals(i.Email, x.Email, StringComparison.OrdinalIgnoreCase)
                );
                if (existInbox == null)
                {
                    recipients.RemoveAll(recipient => ReferenceEquals(recipient.Inbox, x));
                    return;
                }
                x.Id = existInbox.Id;
            });

            // 生成发件项与收件箱对应关系
            var sendingIitemes = await GenerateSendingItems(recipients);

            // 保存到数据库中
            db.SendingItems.AddRange(sendingIitemes);
            await db.SaveChangesAsync();

            var userInfo = await db
                .Users.AsNoTracking()
                .Where(x => x.Id == group.UserId)
                .FirstAsync();

            // 保存关系
            var sendingItemInboxRelations = new List<SendingItemInbox>();
            foreach (var sendingItem in sendingIitemes)
            {
                // 添加组织 id
                sendingItem.OrganizationId = userInfo.OrganizationId;

                var temps = new List<SendingItemInbox>();
                sendingItem.Inboxes?.ForEach(inbox =>
                {
                    var sendingItemInbox = new SendingItemInbox()
                    {
                        SendingItemId = sendingItem.Id,
                        InboxId = inbox.Id,
                        ToEmail = inbox.Email,
                        Role = InboxRole.Recipient
                    };
                    temps.Add(sendingItemInbox);
                });

                sendingItem.CC?.ForEach(inbox =>
                {
                    var sendingItemInbox = new SendingItemInbox()
                    {
                        SendingItemId = sendingItem.Id,
                        InboxId = inbox.Id,
                        ToEmail = inbox.Email,
                        Role = InboxRole.CC
                    };
                    temps.Add(sendingItemInbox);
                });

                sendingItem.BCC?.ForEach(inbox =>
                {
                    var sendingItemInbox = new SendingItemInbox()
                    {
                        SendingItemId = sendingItem.Id,
                        InboxId = inbox.Id,
                        ToEmail = inbox.Email,
                        Role = InboxRole.BCC
                    };
                    temps.Add(sendingItemInbox);
                });

                // 更新搜索关键字
                sendingItem.ToEmails = string.Join(",", temps.Select(x => x.ToEmail));
                sendingItemInboxRelations.AddRange(temps);
            }
            db.SendingItemInboxes.AddRange(sendingItemInboxRelations);
            await db.SaveChangesAsync();

            return sendingIitemes;
        }

        /// <summary>
        /// 获取收件人及其对应的 Excel 行数据。
        /// </summary>
        /// <returns></returns>
        private async Task<List<SendingRecipient>> GetAllRecipients()
        {
            List<EmailAddress> inboxes = [];
            inboxes.AddRange(group.Inboxes);

            // 按组添加
            if (group.InboxGroups?.Count > 0)
            {
                var groupIds = group.InboxGroups.Select(x => x.Id).ToList();
                var invalidInboxEmails = db
                    .Inboxes.IgnoreQueryFilters()
                    .Where(x =>
                        x.OrganizationId == organizationId && x.Status == InboxStatus.Invalid
                    )
                    .Select(x => x.Email);
                var temps = await db
                    .Inboxes.AsNoTracking()
                    .Where(x =>
                        groupIds.Contains(x.EmailGroupId) && !invalidInboxEmails.Contains(x.Email)
                    )
                    .ToListAsync();
                inboxes.AddRange(
                    temps.ConvertAll(x =>
                    {
                        return new EmailAddress()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Email = x.Email
                        };
                    })
                );
            }

            var recipients = inboxes
                .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SendingRecipient(x.First()))
                .ToList();
            if (group.Data == null)
                return recipients;

            var recipientByEmail = recipients.ToDictionary(
                x => x.Inbox.Email,
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var data in group.Data.OfType<JObject>())
            {
                var row = new SendingItemExcelData(data);
                if (string.IsNullOrEmpty(row.Inbox))
                    continue;

                if (recipientByEmail.TryGetValue(row.Inbox, out var existingRecipient))
                {
                    if (existingRecipient.ExcelData == null)
                    {
                        existingRecipient.ExcelData = row;
                        continue;
                    }

                    if (!allowDuplicateSending)
                        continue;
                }

                var recipient = new SendingRecipient(
                    new EmailAddress { Email = row.Inbox, Name = row.InboxName },
                    row
                );
                recipients.Add(recipient);
                recipientByEmail.TryAdd(row.Inbox, recipient);
            }

            return recipients;
        }

        /// <summary>
        /// 开始生成发送项
        /// 当满足以下条件时，对收件人进行合并处理
        /// 1-发件人只有一个
        /// 2-没有数据
        /// 3-只有一个模板或者没有模板
        /// </summary>
        /// <param name="inboxes"></param>
        /// <returns></returns>
        private async Task<List<SendingItem>> GenerateSendingItems(
            List<SendingRecipient> recipients
        )
        {
            // 合并发件的情况
            if (
                group.SendBatch
                && group.Outboxes.Count == 1
                && (group.Data == null || group.Data.Count == 0)
                && (group.Templates == null || group.Templates.Count <= 1)
            )
            {
                // 批量发送时，设置按最大批量进行分割
                // 批量数包含抄送和密送
                int actualBatchSize = _batchSize;
                if (group.CcBoxes != null)
                {
                    actualBatchSize -= group.CcBoxes.Count;
                }
                if (group.BccBoxes != null)
                {
                    actualBatchSize -= group.BccBoxes.Count;
                }
                actualBatchSize = Math.Max(1, actualBatchSize);

                // 分批发送
                List<SendingItem> sendingItemsResult = [];
                int total = 0;
                while (total < recipients.Count)
                {
                    var inboxesTemp = recipients
                        .Skip(total)
                        .Take(actualBatchSize)
                        .Select(x => x.Inbox)
                        .ToList();
                    total += inboxesTemp.Count;

                    var sendingItem = new SendingItem()
                    {
                        // TODO: 因为只有发件有一个时，才会被合并，后期考虑优化
                        OutBoxId = group.Outboxes[0].Id,
                        FromEmail = group.Outboxes[0].Email,
                        SendingGroupId = group.Id,
                        UserId = group.UserId,
                        Inboxes = inboxesTemp,
                        CC = group.CcBoxes,
                        BCC = group.BccBoxes,
                        Attachments = group.Attachments,
                        Status = SendingItemStatus.Created,
                        IsSendingBatch = true
                    };
                    sendingItemsResult.Add(sendingItem);
                }
                return sendingItemsResult;
            }

            List<SendingItem> sendingItems = [];
            foreach (var recipient in recipients)
            {
                var sendingItem = new SendingItem()
                {
                    SendingGroupId = group.Id,
                    UserId = group.UserId,
                    // 对于携带变量的情况，仅支持一对一发件
                    Inboxes = [recipient.Inbox],
                    CC = group.CcBoxes,
                    BCC = group.BccBoxes,
                    // 附件
                    Attachments = group.Attachments,
                    Status = SendingItemStatus.Created
                };
                // 在发送时，才会设置具体的模板
                sendingItems.Add(sendingItem);

                var row = recipient.ExcelData;
                if (row == null)
                    continue;

                // 设置发件箱
                sendingItem.OutBoxId = row.OutboxId;
                sendingItem.FromEmail = row.Outbox;
                if (row.OutboxId > 0 && row.ProxyId > 0)
                {
                    // 代理 Id
                    sendingItem.ProxyId = row.ProxyId;
                }

                // 设置数据
                // 抄送
                if (row.CC != null && row.CC.Count > 0)
                {
                    sendingItem.CC ??= [];

                    foreach (var cc in row.CC)
                    {
                        // 如果不存在，则添加
                        if (sendingItem.CC.Any(x => x.Email == cc))
                            continue;
                        sendingItem.CC.Add(new EmailAddress() { Email = cc, Name = cc });
                    }
                }

                // 密送
                if (row.BCC != null && row.BCC.Count > 0)
                {
                    sendingItem.BCC ??= [];

                    foreach (var bcc in row.BCC)
                    {
                        // 如果不存在，则添加
                        if (sendingItem.BCC.Any(x => x.Email == bcc))
                            continue;
                        sendingItem.BCC.Add(new EmailAddress() { Email = bcc, Name = bcc });
                    }
                }

                // 指定模板
                if (row.TemplateId > 0)
                {
                    sendingItem.EmailTemplateId = row.TemplateId;
                }

                // 覆盖正文
                // 正文和模板在 SendGroupTask.Init() 中设置

                // 添加附件
                // 覆盖正文中的附件
                if (row.AttachmentNames != null && row.AttachmentNames.Count > 0)
                {
                    sendingItem.Attachments = row
                        .AttachmentNames.Select(FileStoreService.NormalizeDisplayName)
                        .Distinct(StringComparer.Ordinal)
                        .Select(x => excelAttachments[x])
                        .ToList();
                }

                // 保存数据
                sendingItem.Data = row;
            }

            return sendingItems;
        }

        /// <summary>
        /// 收件人及其可能覆盖默认发送内容的 Excel 行。
        /// </summary>
        private sealed class SendingRecipient(
            EmailAddress inbox,
            SendingItemExcelData? excelData = null
        )
        {
            public EmailAddress Inbox { get; } = inbox;

            public SendingItemExcelData? ExcelData { get; set; } = excelData;
        }
    }
}
