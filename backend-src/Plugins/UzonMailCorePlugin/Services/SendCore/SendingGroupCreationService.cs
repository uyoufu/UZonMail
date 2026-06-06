using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.CorePlugin.Database.SQL.EmailSending;
using UZonMail.CorePlugin.Services.Emails;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.CorePlugin.Utils.Extensions;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore
{
    public class SendingGroupCreationService(
        SqlContext db,
        TokenService tokenService,
        AppSettingsManager settingsService,
        OutboxValidateService outboxValidateService
    ) : ISendingGroupCreationService, IScopedService<ISendingGroupCreationService>
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(SendingGroupCreationService)
        );

        public async Task<SendingGroup> CreateSendingGroup(SendingGroup sendingGroupData)
        {
            _logger.Info("开始创建发送任务");

            var userId = sendingGroupData.UserId;
            if (sendingGroupData.UserId <= 0 && tokenService != null)
            {
                userId = tokenService.GetUserSqlId();
            }

            sendingGroupData.Data = await FormatExcelData(sendingGroupData.Data, userId);
            await ValidateOutboxes(userId, sendingGroupData);

            await db.RunTransaction(async ctx =>
            {
                if (sendingGroupData.Templates != null)
                {
                    var templateIds = sendingGroupData.Templates.Select(t => t.Id).ToList();
                    sendingGroupData.Templates = ctx
                        .EmailTemplates.Where(x => templateIds.Contains(x.Id))
                        .ToList();
                }

                if (sendingGroupData.Outboxes != null)
                {
                    var outboxIds = sendingGroupData.Outboxes.Select(t => t.Id).ToList();
                    sendingGroupData.Outboxes = ctx
                        .Outboxes.Where(x => outboxIds.Contains(x.Id))
                        .ToList();
                    sendingGroupData.OutboxesCount = sendingGroupData.Outboxes.Count;
                }

                if (sendingGroupData.Attachments != null)
                {
                    var fileUsageIds = sendingGroupData
                        .Attachments.Select(x => x.__fileUsageId)
                        .Where(x => x > 0)
                        .ToList();
                    sendingGroupData.Attachments =
                        fileUsageIds.Count > 0
                            ? await ctx.FileUsages.Where(x => fileUsageIds.Contains(x.Id)).ToListAsync()
                            : [];
                }

                sendingGroupData.Status = SendingGroupStatus.Created;
                sendingGroupData.TotalCount = sendingGroupData.Inboxes.Count;
                sendingGroupData.UserId = userId;
                ctx.SendingGroups.Add(sendingGroupData);
                await ctx.SaveChangesAsync();

                var orgSetting = await settingsService.GetSetting<SendingSetting>(
                    ctx,
                    sendingGroupData.UserId
                );

                await SaveInboxes(sendingGroupData.Data, sendingGroupData.UserId);

                var builder = new SendingItemsBuilder(
                    ctx,
                    sendingGroupData,
                    orgSetting.MaxSendingBatchSize
                );
                var items = await builder.GenerateAndSave();

                sendingGroupData.TotalCount = items.Count;
                await UpdateOutboxCountFromGroups(sendingGroupData);
                await IncreaseAttachmentLinkCount(ctx, items);

                return await ctx.SaveChangesAsync();
            });

            _logger.Info("发送任务创建成功");
            return sendingGroupData;
        }

        private async Task<JArray?> FormatExcelData(JArray? data, long userId)
        {
            if (data == null || data.Count == 0)
            {
                return data;
            }

            var outboxEmails = data.Select(x => x.SelectTokenOrDefault("outbox", ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var outboxes = await db
                .Outboxes.Where(x => x.UserId == userId && outboxEmails.Contains(x.Email))
                .ToListAsync();

            var templateIds = data.Select(x => x.SelectTokenOrDefault("templateId", 0L))
                .Where(x => x > 0)
                .ToList();
            var templateNames = data.Select(x => x.SelectTokenOrDefault("templateName", ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var templates = await db
                .EmailTemplates.Where(x =>
                    x.UserId == userId
                    && (templateIds.Contains(x.Id) || templateNames.Contains(x.Name))
                )
                .ToListAsync();

            JArray results = [];
            foreach (var token in data)
            {
                var outboxEmail = token.SelectTokenOrDefault("outbox", "");
                if (!string.IsNullOrEmpty(outboxEmail))
                {
                    var outboxEntity = outboxes.FirstOrDefault(x => x.Email == outboxEmail);
                    if (outboxEntity != null)
                    {
                        token["outboxId"] = outboxEntity.Id;
                    }
                    else
                    {
                        token["outboxId"] = 0;
                        token["outbox"] = string.Empty;
                    }
                }

                var templateId = token.SelectTokenOrDefault("templateId", 0);
                var templateName = token.SelectTokenOrDefault("templateName", "");
                var templateEntity = templates.FirstOrDefault(x =>
                    x.Id == templateId || x.Name == templateName
                );

                token["templateId"] = templateEntity?.Id ?? 0;
                results.Add(token);
            }

            return results;
        }

        private async Task ValidateOutboxes(long userId, SendingGroup sendingGroupData)
        {
            _logger.Debug("开始验证发件箱");
            var outboxIds = sendingGroupData.Outboxes?.Select(x => x.Id).ToList() ?? [];
            var groupOutboxIds = sendingGroupData.OutboxGroups?.Select(x => x.Id).ToList() ?? [];
            var dataOutboxIds =
                sendingGroupData
                    .Data?.Select(x => x.SelectTokenOrDefault("outboxId", ""))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => long.TryParse(x, out var id) ? id : 0)
                    .Where(x => x > 0) ?? [];
            var dataOutboxEmails =
                sendingGroupData
                    .Data?.Select(x => x.SelectTokenOrDefault("outbox", ""))
                    .Where(x => !string.IsNullOrEmpty(x)) ?? [];

            var allOutboxIds = outboxIds.Concat(dataOutboxIds).ToList();
            var outboxes = await db
                .Outboxes.AsNoTracking()
                .Where(x => x.Status != OutboxStatus.Valid)
                .Where(x =>
                    allOutboxIds.Contains(x.Id)
                    || dataOutboxEmails.Contains(x.Email)
                    || groupOutboxIds.Contains(x.EmailGroupId)
                )
                .ToListAsync();

            foreach (var outbox in outboxes)
            {
                var result = await outboxValidateService.ValidateOutbox(outbox);
                if (result.NotOk)
                {
                    throw new KnownException($"发件箱 {outbox.Email} 验证失败: {result.Message}");
                }
            }

            _logger.Debug("发件箱验证通过");
        }

        private async Task SaveInboxes(JArray? data, long userId)
        {
            if (data == null)
                return;

            var emails = data.Select(x => x["inbox"])
                .Where(x => x != null)
                .Select(x => x!.ToString())
                .ToList();
            if (emails.Count == 0)
                return;

            var existsEmails = await db
                .Inboxes.AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId && emails.Contains(x.Email))
                .Select(x => x.Email)
                .ToListAsync();

            var newEmails = emails.Except(existsEmails);
            var defaultInboxGroup = await db
                .EmailGroups.Where(x =>
                    x.UserId == userId && x.Type == EmailGroupType.InBox && x.IsDefault
                )
                .FirstOrDefaultAsync();
            if (defaultInboxGroup == null)
            {
                defaultInboxGroup = EmailGroup.GetDefaultEmailGroup(userId, EmailGroupType.InBox);
                db.EmailGroups.Add(defaultInboxGroup);
                await db.SaveChangesAsync();
            }

            await db
                .Inboxes.IgnoreQueryFilters()
                .Where(x => x.UserId == userId && x.IsDeleted && emails.Contains(x.Email))
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, false));

            foreach (var email in newEmails)
            {
                var inbox = new Inbox()
                {
                    Email = email,
                    UserId = userId,
                    EmailGroupId = defaultInboxGroup.Id
                };
                inbox.SetStatusNormal();
                db.Inboxes.Add(inbox);
            }

            await db.SaveChangesAsync();
        }

        private async Task UpdateOutboxCountFromGroups(SendingGroup sendingGroupData)
        {
            if (sendingGroupData.OutboxGroups == null || sendingGroupData.OutboxGroups.Count == 0)
                return;

            var outboxGroupIds = sendingGroupData.OutboxGroups.Select(x => x.Id).ToList();
            var outboxCount = await db
                .Outboxes.AsNoTracking()
                .Where(x => outboxGroupIds.Contains(x.EmailGroupId))
                .CountAsync();
            sendingGroupData.OutboxesCount += outboxCount;
        }

        private static async Task IncreaseAttachmentLinkCount(SqlContext ctx, List<SendingItem> items)
        {
            var incInfos = items
                .Select(x => x.Attachments)
                .Where(x => x != null)
                .SelectMany(x => x!)
                .Select(x => x.FileObjectId)
                .GroupBy(x => x)
                .Select(x => new { count = x.Count(), fileObjectId = x.Key })
                .ToList();

            foreach (var info in incInfos)
            {
                await ctx
                    .FileObjects.Where(x => x.Id == info.fileObjectId)
                    .ExecuteUpdateAsync(x =>
                        x.SetProperty(e => e.LinkCount, e => e.LinkCount + info.count)
                    );
            }
        }
    }
}
