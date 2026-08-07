using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Database.SQL.EmailSending;
using UzonMail.CorePlugin.Services.Emails;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Json;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore
{
    public class SendingGroupCreationService(
        SqlContext db,
        TokenService tokenService,
        AppSettingsManager settingsService,
        OutboxValidateService outboxValidateService,
        FileReferenceService fileReferenceService
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
            var organizationId = await db
                .Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.OrganizationId)
                .SingleAsync();
            await FilterInvalidInboxes(sendingGroupData, organizationId);
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
                            ? await ctx
                                .FileUsages.Where(x =>
                                    fileUsageIds.Contains(x.Id) && x.OwnerUserId == userId
                                )
                                .ToListAsync()
                            : [];
                }

                var orgSetting = await settingsService.GetSetting<SendingSetting>(ctx, userId);
                ValidateDuplicateExcelRecipients(
                    sendingGroupData.Data,
                    orgSetting.AllowDuplicateSending
                );
                var excelAttachments = await ResolveExcelAttachments(
                    ctx,
                    sendingGroupData.Data,
                    userId
                );

                sendingGroupData.Status = SendingGroupStatus.Created;
                sendingGroupData.TotalCount = sendingGroupData.Inboxes.Count;
                sendingGroupData.UserId = userId;
                ctx.SendingGroups.Add(sendingGroupData);
                await ctx.SaveChangesAsync();

                await SaveInboxes(sendingGroupData.Data, sendingGroupData.UserId, organizationId);

                var builder = new SendingItemsBuilder(
                    ctx,
                    sendingGroupData,
                    orgSetting.MaxSendingBatchSize,
                    orgSetting.AllowDuplicateSending,
                    excelAttachments,
                    organizationId
                );
                var items = await builder.GenerateAndSave();
                if (items.Count == 0)
                    throw new KnownException("没有可发送的有效收件箱");

                sendingGroupData.TotalCount = items.Count;
                await UpdateOutboxCountFromGroups(sendingGroupData);
                await fileReferenceService.IncreaseReferencesAsync(items);

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
                if (token is not JObject)
                    continue;

                var inboxEmail = (
                    token.SelectTokenOrDefault("inbox", string.Empty) ?? string.Empty
                ).Trim();
                if (!string.IsNullOrEmpty(inboxEmail))
                {
                    token["inbox"] = inboxEmail;
                }

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

        /// <summary>
        /// 移除直选和 Excel 中已被当前组织标记为无效的收件箱
        /// 收件组在生成发送项时展开，因此由生成器使用同一组织条件过滤
        /// </summary>
        private async Task FilterInvalidInboxes(SendingGroup sendingGroupData, long organizationId)
        {
            var inboxEmails = sendingGroupData
                .Inboxes.Select(x => x.Email)
                .Concat(
                    sendingGroupData
                        .Data?.OfType<JObject>()
                        .Select(x => x.SelectTokenOrDefault("inbox", string.Empty) ?? string.Empty)
                        ?? []
                )
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (inboxEmails.Count == 0)
                return;

            var invalidEmails = (
                await db
                    .Inboxes.AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(x =>
                        x.OrganizationId == organizationId
                        && x.Status == InboxStatus.Invalid
                        && inboxEmails.Contains(x.Email)
                    )
                    .Select(x => x.Email)
                    .ToListAsync()
            ).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (invalidEmails.Count == 0)
                return;

            sendingGroupData.Inboxes.RemoveAll(x => invalidEmails.Contains(x.Email));
            foreach (var data in sendingGroupData.Data?.OfType<JObject>().ToList() ?? [])
            {
                var inboxEmail = data.SelectTokenOrDefault("inbox", string.Empty) ?? string.Empty;
                if (invalidEmails.Contains(inboxEmail.Trim()))
                    data.Remove();
            }
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

        /// <summary>
        /// 验证 Excel 中是否包含重复收件人。
        /// </summary>
        private static void ValidateDuplicateExcelRecipients(
            JArray? data,
            bool allowDuplicateSending
        )
        {
            if (allowDuplicateSending || data == null || data.Count == 0)
                return;

            var duplicateRecipients = data.OfType<JObject>()
                .Select(x => (x.SelectTokenOrDefault("inbox", string.Empty) ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(x => new { Email = x.First(), Count = x.Count() })
                .Where(x => x.Count > 1)
                .ToList();
            if (duplicateRecipients.Count == 0)
                return;

            var duplicateDetails = string.Join(
                ", ",
                duplicateRecipients.Select(x => $"{x.Email} ({x.Count} 次)")
            );
            throw new KnownException(
                $"Excel 中存在重复收件人：{duplicateDetails}。当前未开启“允许重复发件”，请删除重复项或开启该设置。"
            );
        }

        /// <summary>
        /// 解析并验证 Excel 中指定的附件名称。
        /// </summary>
        private static async Task<Dictionary<string, FileUsage>> ResolveExcelAttachments(
            SqlContext context,
            JArray? data,
            long userId
        )
        {
            var attachmentNameKeys =
                data?.OfType<JObject>()
                    .SelectMany(x => new SendingItemExcelData(x).AttachmentNames)
                    .Select(FileStoreService.NormalizeDisplayName)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList() ?? [];
            if (attachmentNameKeys.Count == 0)
                return [];

            var candidates = await context
                .FileUsages.Where(x =>
                    attachmentNameKeys.Contains(x.DisplayNameKey!)
                    && (x.OwnerUserId == userId || x.IsPublic)
                )
                .ToListAsync();
            Dictionary<string, FileUsage> resolved = [];
            List<string> unresolvedNames = [];

            foreach (var attachmentNameKey in attachmentNameKeys)
            {
                var ownedCandidates = candidates
                    .Where(x => x.DisplayNameKey == attachmentNameKey && x.OwnerUserId == userId)
                    .ToList();
                var matchingCandidates =
                    ownedCandidates.Count > 0
                        ? ownedCandidates
                        : candidates
                            .Where(x =>
                                x.DisplayNameKey == attachmentNameKey
                                && x.OwnerUserId != userId
                                && x.IsPublic
                            )
                            .ToList();
                if (matchingCandidates.Count != 1)
                {
                    unresolvedNames.Add(attachmentNameKey);
                    continue;
                }

                resolved.Add(attachmentNameKey, matchingCandidates[0]);
            }

            if (unresolvedNames.Count > 0)
            {
                throw new KnownException(
                    $"以下 Excel 附件不存在或名称不唯一: {string.Join(", ", unresolvedNames)}"
                );
            }

            return resolved;
        }

        private async Task SaveInboxes(JArray? data, long userId, long organizationId)
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
                    OrganizationId = organizationId,
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
    }
}
