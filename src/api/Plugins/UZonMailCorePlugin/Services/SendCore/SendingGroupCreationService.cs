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
        SenderAccountValidateService senderAccountValidateService,
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
            await FilterInvalidRecipients(sendingGroupData, organizationId);
            await ValidateSenderAccounts(userId, sendingGroupData);

            await db.RunTransaction(async ctx =>
            {
                if (sendingGroupData.Templates != null)
                {
                    var templateIds = sendingGroupData.Templates.Select(t => t.Id).ToList();
                    sendingGroupData.Templates = ctx
                        .EmailTemplates.Where(x => templateIds.Contains(x.Id))
                        .ToList();
                }

                if (sendingGroupData.SenderAccounts != null)
                {
                    var senderAccountIds = sendingGroupData
                        .SenderAccounts.Select(t => t.Id)
                        .ToList();
                    sendingGroupData.SenderAccounts = ctx
                        .SenderAccounts.Where(x => senderAccountIds.Contains(x.Id))
                        .ToList();
                    sendingGroupData.SenderAccountCount = sendingGroupData.SenderAccounts.Count;
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
                sendingGroupData.TotalCount = sendingGroupData.Recipients.Count;
                sendingGroupData.UserId = userId;
                ctx.SendingGroups.Add(sendingGroupData);
                await ctx.SaveChangesAsync();

                await SaveRecipients(
                    sendingGroupData.Data,
                    sendingGroupData.UserId,
                    organizationId
                );

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
                await UpdateSenderAccountCountFromGroups(sendingGroupData);
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

            var senderEmails = data.Select(x => x.SelectTokenOrDefault("senderEmail", ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var senderAccounts = await db
                .SenderAccounts.Include(x => x.EmailAccount)
                .Where(x =>
                    x.EmailAccount.UserId == userId && senderEmails.Contains(x.EmailAccount.Email)
                )
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

                var recipientEmail = (
                    token.SelectTokenOrDefault("recipientEmail", string.Empty) ?? string.Empty
                ).Trim();
                if (!string.IsNullOrEmpty(recipientEmail))
                {
                    token["recipientEmail"] = recipientEmail;
                }

                var senderEmail = token.SelectTokenOrDefault("senderEmail", "");
                if (!string.IsNullOrEmpty(senderEmail))
                {
                    var senderAccount = senderAccounts.FirstOrDefault(x => x.Email == senderEmail);
                    if (senderAccount != null)
                    {
                        token["senderAccountId"] = senderAccount.Id;
                    }
                    else
                    {
                        token["senderAccountId"] = 0;
                        token["senderEmail"] = string.Empty;
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
        private async Task FilterInvalidRecipients(
            SendingGroup sendingGroupData,
            long organizationId
        )
        {
            var recipientEmails = sendingGroupData
                .Recipients.Select(x => x.Email)
                .Concat(
                    sendingGroupData
                        .Data?.OfType<JObject>()
                        .Select(x =>
                            x.SelectTokenOrDefault("recipientEmail", string.Empty) ?? string.Empty
                        ) ?? []
                )
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (recipientEmails.Count == 0)
                return;

            var invalidEmails = (
                await db
                    .RecipientContacts.AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(x =>
                        x.OrganizationId == organizationId
                        && x.ValidationStatus == RecipientValidationStatus.Invalid
                        && recipientEmails.Contains(x.Email)
                    )
                    .Select(x => x.Email)
                    .ToListAsync()
            ).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (invalidEmails.Count == 0)
                return;

            sendingGroupData.Recipients.RemoveAll(x => invalidEmails.Contains(x.Email));
            foreach (var data in sendingGroupData.Data?.OfType<JObject>().ToList() ?? [])
            {
                var recipientEmail =
                    data.SelectTokenOrDefault("recipientEmail", string.Empty) ?? string.Empty;
                if (invalidEmails.Contains(recipientEmail.Trim()))
                    data.Remove();
            }
        }

        private async Task ValidateSenderAccounts(long userId, SendingGroup sendingGroupData)
        {
            _logger.Debug("开始验证发件箱");
            var senderAccountIds =
                sendingGroupData.SenderAccounts?.Select(x => x.Id).ToList() ?? [];
            var groupSenderAccountIds =
                sendingGroupData.SenderAccountGroups?.Select(x => x.Id).ToList() ?? [];
            var dataSenderAccountIds =
                sendingGroupData
                    .Data?.Select(x => x.SelectTokenOrDefault("senderAccountId", ""))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => long.TryParse(x, out var id) ? id : 0)
                    .Where(x => x > 0) ?? [];
            var excelSenderEmails =
                sendingGroupData
                    .Data?.Select(x => x.SelectTokenOrDefault("senderEmail", ""))
                    .Where(x => !string.IsNullOrEmpty(x)) ?? [];

            var allSenderAccountIds = senderAccountIds.Concat(dataSenderAccountIds).ToList();
            var invalidSenderAccounts = await db
                .SenderAccounts.AsNoTracking()
                .Where(x => x.Status != SenderAccountStatus.Valid)
                .Where(x =>
                    allSenderAccountIds.Contains(x.Id)
                    || excelSenderEmails.Contains(x.EmailAccount.Email)
                    || groupSenderAccountIds.Contains(x.EmailAccount.EmailGroupId)
                )
                .ToListAsync();

            foreach (var senderAccount in invalidSenderAccounts)
            {
                var result = await senderAccountValidateService.ValidateSenderAccount(
                    senderAccount
                );
                if (result.NotOk)
                {
                    throw new KnownException($"发件账户 {senderAccount.Email} 验证失败: {result.Message}");
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
                .Select(x =>
                    (x.SelectTokenOrDefault("recipientEmail", string.Empty) ?? string.Empty).Trim()
                )
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

        private async Task SaveRecipients(JArray? data, long userId, long organizationId)
        {
            if (data == null)
                return;

            var emails = data.Select(x => x["recipientEmail"])
                .Where(x => x != null)
                .Select(x => x!.ToString())
                .ToList();
            if (emails.Count == 0)
                return;

            var existsEmails = await db
                .RecipientContacts.AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId && emails.Contains(x.Email))
                .Select(x => x.Email)
                .ToListAsync();

            var newEmails = emails.Except(existsEmails);
            var defaultRecipientContactGroup = await db
                .EmailGroups.Where(x =>
                    x.UserId == userId
                    && x.Category == EmailGroupCategory.RecipientEmail
                    && x.IsDefault
                )
                .FirstOrDefaultAsync();
            if (defaultRecipientContactGroup == null)
            {
                defaultRecipientContactGroup = EmailGroup.GetDefaultEmailGroup(
                    userId,
                    EmailGroupCategory.RecipientEmail
                );
                db.EmailGroups.Add(defaultRecipientContactGroup);
                await db.SaveChangesAsync();
            }

            await db
                .RecipientContacts.IgnoreQueryFilters()
                .Where(x => x.UserId == userId && x.IsDeleted && emails.Contains(x.Email))
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, false));

            foreach (var email in newEmails)
            {
                var recipientContact = new RecipientContact()
                {
                    Email = email,
                    UserId = userId,
                    OrganizationId = organizationId,
                    EmailGroupId = defaultRecipientContactGroup.Id
                };
                recipientContact.ValidationStatus = RecipientValidationStatus.Unverified;
                db.RecipientContacts.Add(recipientContact);
            }

            await db.SaveChangesAsync();
        }

        private async Task UpdateSenderAccountCountFromGroups(SendingGroup sendingGroupData)
        {
            if (
                sendingGroupData.SenderAccountGroups == null
                || sendingGroupData.SenderAccountGroups.Count == 0
            )
                return;

            var senderAccountGroupIds = sendingGroupData
                .SenderAccountGroups.Select(x => x.Id)
                .ToList();
            var senderAccountCount = await db
                .SenderAccounts.AsNoTracking()
                .Where(x => senderAccountGroupIds.Contains(x.EmailAccount.EmailGroupId))
                .CountAsync();
            sendingGroupData.SenderAccountCount += senderAccountCount;
        }
    }
}
