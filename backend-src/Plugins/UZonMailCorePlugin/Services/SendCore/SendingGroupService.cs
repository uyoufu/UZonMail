using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Quartz;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Database.SQL.EmailSending;
using UZonMail.CorePlugin.Database.Validators;
using UZonMail.CorePlugin.Jobs;
using UZonMail.CorePlugin.Services.Config;
using UZonMail.CorePlugin.Services.Emails;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.Sender.Smtp;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.CorePlugin.Utils.Extensions;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore
{
    /// <summary>
    /// 发送组服务
    /// </summary>
    public class SendingGroupService(
        SqlContext db,
        DebugConfig debugConfig,
        TokenService tokenService,
        ISendingTasksManager tasksService,
        GroupTasksManager waitList,
        OutboxesManager outboxesManager,
        SmtpClientsManager clientFactory,
        AppSettingsManager settingsService,
        ISchedulerFactory schedulerFactory,
        IServiceProvider serviceProvider
    ) : IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SendingGroupService));

        /// <summary>
        /// 创建发送组
        /// </summary>
        /// <param name="sendingGroupData">该对象要求没有被ef跟踪</param>
        /// <returns></returns>
        public async Task<SendingGroup> CreateSendingGroup(SendingGroup sendingGroupData)
        {
            _logger.Info("开始创建发送任务");

            var userId = sendingGroupData.UserId;
            if (sendingGroupData.UserId <= 0 && tokenService != null)
            {
                userId = tokenService.GetUserSqlId();
            }

            // 格式化 Excel 数据
            sendingGroupData.Data = await FormatExcelData(sendingGroupData.Data, userId);

            // 进行发件箱的数据验证，验证失败时
            await ValidateOutboxes(userId, sendingGroupData);

            // 使用事务
            await db.RunTransaction(async ctx =>
            {
                // 添加数据
                // 跟踪数据，将数据转换成 EF 对象
                if (sendingGroupData.Templates != null)
                {
                    var templates = ctx
                        .EmailTemplates.Where(x =>
                            sendingGroupData.Templates.Select(t => t.Id).Contains(x.Id)
                        )
                        .ToList();
                    sendingGroupData.Templates = templates;
                }
                if (sendingGroupData.Outboxes != null)
                {
                    var outboxes = ctx
                        .Outboxes.Where(x =>
                            sendingGroupData.Outboxes.Select(t => t.Id).Contains(x.Id)
                        )
                        .ToList();
                    sendingGroupData.Outboxes = outboxes;
                    sendingGroupData.OutboxesCount = outboxes.Count;
                }
                if (sendingGroupData.Attachments != null)
                {
                    var fileUsageIds = sendingGroupData
                        .Attachments.Select(x => x.__fileUsageId)
                        .Where(x => x > 0)
                        .ToList();
                    if (fileUsageIds.Count > 0)
                    {
                        var attachmenets = await ctx
                            .FileUsages.Where(x => fileUsageIds.Contains(x.Id))
                            .ToListAsync();
                        sendingGroupData.Attachments = attachmenets;
                    }
                    else
                        sendingGroupData.Attachments = [];
                }
                // 增加数据
                sendingGroupData.Status = SendingGroupStatus.Created;
                // 解析总数
                sendingGroupData.TotalCount = sendingGroupData.Inboxes.Count;
                sendingGroupData.UserId = userId;
                ctx.SendingGroups.Add(sendingGroupData);
                // 保存 group，从而获取 Id
                await ctx.SaveChangesAsync();

                // 获取用户设置
                var orgSetting = await settingsService.GetSetting<SendingSetting>(
                    ctx,
                    sendingGroupData.UserId
                );

                // 保存发件箱
                await SaveInboxes(sendingGroupData.Data, sendingGroupData.UserId);

                // 将数据组装成 SendingItem 保存
                // 要确保数据已经通过验证
                var builder = new SendingItemsBuilder(
                    ctx,
                    sendingGroupData,
                    orgSetting.MaxSendingBatchSize
                );
                List<SendingItem> items = await builder.GenerateAndSave();

                // 更新发件总数量
                sendingGroupData.TotalCount = items.Count;
                // 更新发件箱的数量
                if (
                    sendingGroupData.OutboxGroups != null
                    && sendingGroupData.OutboxGroups.Count > 0
                )
                {
                    var outboxGroupIds = sendingGroupData.OutboxGroups.Select(x => x.Id).ToList();
                    var outboxCount = await db
                        .Outboxes.AsNoTracking()
                        .Where(x => outboxGroupIds.Contains(x.EmailGroupId))
                        .CountAsync();
                    sendingGroupData.OutboxesCount += outboxCount;
                }

                // 增加附件使用记录
                var incInfos = items
                    .Select(x => x.Attachments)
                    .Where(x => x != null)
                    .SelectMany(x => x)
                    .Select(x => x.FileObjectId)
                    .GroupBy(x => x)
                    .Select(x => new { count = x.Count(), fileObjectId = x.Key })
                    .ToList();
                foreach (var info in incInfos)
                {
                    await ctx
                        .FileObjects.Where(x => x.Id == info.fileObjectId)
                        .ExecuteUpdateAsync(x =>
                            x.SetProperty(e => e.LinkCount, e => e.LinkCount + 1)
                        );
                }

                return await ctx.SaveChangesAsync();
            });

            _logger.Info("发送任务创建成功");
            return sendingGroupData;
        }

        /// <summary>
        /// 格式化 Excel 数据
        /// 只保留属于自己的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<JArray?> FormatExcelData(JArray? data, long userId)
        {
            if (data == null || data.Count == 0)
            {
                return data;
            }

            // 获取发件箱,只能使用自己名下的发件箱
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

            // 重新更新数据
            JArray results = [];
            foreach (var token in data)
            {
                var outboxEmail = token.SelectTokenOrDefault("outbox", "");
                if (!string.IsNullOrEmpty(outboxEmail))
                {
                    // 获取 outboxId
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

                if (templateEntity == null)
                {
                    token["templateId"] = 0;
                }
                else
                {
                    token["templateId"] = templateEntity.Id;
                }
                results.Add(token);
            }

            return results;
        }

        /// <summary>
        /// 验证当前组中的所有发件箱
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sendingGroupData"></param>
        /// <returns></returns>
        private async Task ValidateOutboxes(long userId, SendingGroup sendingGroupData)
        {
            _logger.Debug("开始验证发件箱");
            var outboxIds = sendingGroupData.Outboxes.Select(x => x.Id).ToList();
            var groupOutboxIds = sendingGroupData.OutboxGroups?.Select(x => x.Id).ToList() ?? [];
            var dataOutboxIds =
                sendingGroupData
                    .Data?.Select(x => x.SelectTokenOrDefault("outboxId", ""))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => long.Parse(x)) ?? [];
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

            // 重新验证发件箱
            var emailUtils = serviceProvider.GetRequiredService<OutboxValidateService>();
            foreach (var outbox in outboxes)
            {
                var result = await emailUtils.ValidateOutbox(outbox);
                if (result.NotOk)
                {
                    throw new KnownException($"发件箱 {outbox.Email} 验证失败: {result.Message}");
                }
            }
            _logger.Debug("发件箱验证通过");
        }

        /// <summary>
        /// 保存数据中的收件箱
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task SaveInboxes(JArray? data, long userId)
        {
            if (data == null)
                return;
            var emails = data.Select(x => x["inbox"])
                .Where(x => x != null)
                .Select(x => x.ToString())
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
            // 新建 email
            // 查找默认的收件组
            var defaultInboxGroup = await db
                .EmailGroups.Where(x => x.Type == EmailGroupType.InBox && x.IsDefault)
                .FirstOrDefaultAsync();
            if (defaultInboxGroup == null)
            {
                defaultInboxGroup = EmailGroup.GetDefaultEmailGroup(userId, EmailGroupType.InBox);
                db.EmailGroups.Add(defaultInboxGroup);
                await db.SaveChangesAsync();
            }

            // 某些发件箱可能被删除，恢复数据
            await db
                .Inboxes.IgnoreQueryFilters()
                .Where(x => x.UserId == userId && x.IsDeleted && emails.Contains(x.Email))
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, false));

            // 新建发件箱
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

        /// <summary>
        /// 立即发件
        /// sendingGroup 需要提供 SmtpPasswordSecretKeys 参数
        /// 所有的发送都从该接口触发
        /// </summary>
        /// <param name="sendingGroup"></param>
        /// <param name="sendItemIds">若有值，则只会发送这部分邮件</param>
        /// <returns></returns>
        public async Task SendNow(SendingGroup sendingGroup, List<long>? sendItemIds = null)
        {
            if (debugConfig.IsDemo)
            {
                // 限制最多只能发 10 条
                var sendingImtesCount = await db.SendingItems.CountAsync(x =>
                    x.SendingGroupId == sendingGroup.Id
                );
                if (sendingImtesCount > 5)
                {
                    throw new KnownException("示例环境最多群发 5 条");
                }
            }

            // 创建新的上下文
            var sendingContext = serviceProvider.GetRequiredService<SendingContext>();
            // 添加到发件列表
            await waitList.AddSendingGroup(sendingContext, sendingGroup, sendItemIds);
            // 开始发件
            tasksService.StartSending();
        }

        /// <summary>
        /// 计划发件
        /// </summary>
        /// <param name="sendingGroup"></param>
        /// <returns></returns>
        public async Task SendSchedule(SendingGroup sendingGroup)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var jobKey = new JobKey($"emailSending-{sendingGroup.Id}", "sendingGroup");

            var job = JobBuilder
                .Create<EmailSendingJob>()
                .WithIdentity(jobKey)
                .SetJobData(new JobDataMap { { "sendingGroupId", sendingGroup.Id } })
                .Build();

            // 先指定为 Unspecified，再转为本地时间
            var trigger = TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .StartAt(new DateTimeOffset(sendingGroup.ScheduleDate))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// 立即发件或者计划发件
        /// 根据 scheduleDate 进行判断
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseResult<SendingGroup>> StartSending(SendingGroup sendingData)
        {
            var sendingGroupValidator = new SendingGroupValidator();
            var vdResult = sendingGroupValidator.Validate(sendingData);
            // 校验数据
            if (!vdResult.IsValid)
            {
                return vdResult.ToErrorResponse<SendingGroup>();
            }

            var isSchedule = sendingData.ScheduleDate > DateTime.UtcNow.AddMinutes(1);

            // 创建发件组
            sendingData.SendingType = isSchedule
                ? SendingGroupType.Scheduled
                : SendingGroupType.Instant;
            var sendingGroup = await CreateSendingGroup(sendingData);

            // 判断是立即发件还是计划发件
            if (isSchedule)
            {
                await SendSchedule(sendingGroup);
            }
            else
            {
                await SendNow(sendingGroup);
            }

            return new SendingGroup()
            {
                Id = sendingGroup.Id,
                ObjectId = sendingGroup.ObjectId,
                TotalCount = sendingGroup.TotalCount
            }.ToSuccessResponse();
        }

        /// <summary>
        /// 移除发件任务
        /// 里面不会修改发件组和发件项的状态
        /// 因为移除可能是暂停、停止等不同的状态
        /// </summary>
        /// <param name="sendingGroup"></param>
        /// <param name="cancelMessage"></param>
        /// <returns></returns>
        public async Task RemoveSendingGroupTask(SendingGroup sendingGroup, string removeReason)
        {
            // 若处于发送中，则取消
            if (sendingGroup.Status == SendingGroupStatus.Sending)
            {
                // 找到关联的发件箱移除
                var removedOutboxes = outboxesManager.RemoveOutbox(sendingGroup.Id, removeReason);

                // 移除关联的客户端
                foreach (var outbox in removedOutboxes)
                {
                    // 释放发件箱
                    clientFactory.DisposeSmtpClients(outbox.Email);
                }

                // 移除任务
                waitList.RemoveSendingGroupTask(sendingGroup.UserId, sendingGroup.Id);
            }

            // 若是计划发件，则取消计划
            if (sendingGroup.SendingType == SendingGroupType.Scheduled)
            {
                await RemoveSendSchedule(sendingGroup.Id);
            }
        }

        public async Task UpdateSendingGroupStatus(
            long sendingGroupId,
            SendingGroupStatus status,
            string updateReason = ""
        )
        {
            await UpdateSendingGroupStatus([sendingGroupId], status, updateReason);
        }

        public async Task UpdateSendingGroupStatus(
            List<long> sendingGroupIds,
            SendingGroupStatus status,
            string updateReason = ""
        )
        {
            var sendingItemStatus = status switch
            {
                SendingGroupStatus.Cancel => SendingItemStatus.Cancel,
                SendingGroupStatus.Pause => SendingItemStatus.Failed,
                SendingGroupStatus.Finish => SendingItemStatus.Success,
                _ => SendingItemStatus.Failed,
            };

            // 修改组状态
            await db.SendingGroups.UpdateAsync(
                x => sendingGroupIds.Contains(x.Id),
                x => x.SetProperty(y => y.Status, status)
            );
            // 修改邮件项状态
            await db.SendingItems.UpdateAsync(
                x =>
                    sendingGroupIds.Contains(x.SendingGroupId)
                    && (
                        x.Status == SendingItemStatus.Pending
                        || x.Status == SendingItemStatus.Sending
                    ),
                x =>
                    x.SetProperty(y => y.Status, sendingItemStatus)
                        .SetProperty(y => y.SendResult, updateReason)
            );
        }

        /// <summary>
        /// 移除发件计划
        /// </summary>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public async Task RemoveSendSchedule(long sendingGroupId)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var jobKey = new JobKey($"emailSending-{sendingGroupId}", "sendingGroup");
            await scheduler.DeleteJob(jobKey);
        }
    }
}
