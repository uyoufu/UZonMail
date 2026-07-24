using System.Collections.Concurrent;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.Encrypt.Models;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.EmailWaitList;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Reading;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Configs;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList
{
    /// <summary>
    /// 一次发件任务
    /// 不同的发件任务包含不同的数据库上下文，保证上下文不重复使用
    /// 包含发件组和收件内容
    /// 请调用 <seealso cref="Create(SendingContext, long)"/> 方法创建实例
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class GroupTask(
        EncryptService encryptService,
        ISendItemReaderPool readerPool,
        ISendPayloadReader payloadReader,
        ISendLeaseStore leaseStore,
        ISendingWorkerCoordinator workerCoordinator,
        IAppOptions<SendingQuotaOptions> quotaOptions,
        TimeProvider timeProvider
    ) : ITransientService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupTask));
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private SendItemReaderSession? _reader;
        private bool _startNotified;
        private readonly ConcurrentDictionary<long, DelayedItem> _delayedItems = [];
        private readonly CancellationTokenSource _lifetime = new();
        private int _closed;

        private sealed class DelayedItem(long outboxId, int triedCount)
        {
            public long OutboxId { get; } = outboxId;
            public int TriedCount { get; } = triedCount;
            public Task Task { get; set; } = Task.CompletedTask;
        }

        /// <summary>
        /// 创建一个发件任务
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        public static async Task<GroupTask?> Create(SendingContext ctx, long sendingGroupId)
        {
            var groupTask = ctx.Provider.GetRequiredService<GroupTask>();
            // 初始化基本参数
            groupTask.SetSendingGroupId(sendingGroupId);
            // 初始化组
            if (!await groupTask.InitSendingGroup(ctx))
                return null;
            return groupTask;
        }

        #region 属性
        /// <summary>
        /// 组 id
        /// </summary>
        public long SendingGroupId { get; private set; }

        /// <summary>
        /// 通过组 id 获取的组
        /// </summary>
        private SendingGroup _sendingGroup = null!;

        /// <summary>
        /// 密钥
        /// </summary>
        public EncryptParams SmtpPasswordSecretKeys => encryptService.GetEncrypParams();

        /// <summary>
        /// 所属用户
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 发件项数据
        /// 此处只保存自由发件的项
        /// 若指定发件箱，数据 id 会保存在 outbox 中
        /// </summary>
        private readonly SendingItemMetaList _sendingItemMetas = new();

        /// <summary>
        /// 可用的代理
        /// </summary>
        private List<long> ProxyIds { get; set; } = [];

        /// <summary>
        /// 可用的模板
        /// </summary>
        private UsableTemplateList _usableTemplates = null!;

        /// <summary>
        /// 是否应该释放
        /// </summary>
        public bool ShouldDispose =>
            _sendingItemMetas.Count == 0
            && _delayedItems.IsEmpty
            && _reader is not { IsCompleted: false };

        public int ReadyCount => _sendingItemMetas.WaitListCount;

        public int ActiveCount => _sendingItemMetas.ActiveCount;

        public int DelayedCount => _delayedItems.Count;

        /// <summary>
        /// 任务开始日期
        /// </summary>
        private readonly DateTime _startDate = DateTime.UtcNow;
        #endregion

        #region 初始化
        public void SetSendingGroupId(long sendingGroupId)
        {
            SendingGroupId = sendingGroupId;
        }
        #endregion

        /// <summary>
        /// 初始化发件组信息
        /// 只能被调用一次
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<bool> InitSendingGroup(SendingContext sendingContext)
        {
            var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();

            // 获取完整的邮件组
            var sendingGroup = await sqlContext
                .SendingGroups.AsNoTracking()
                .Where(x => x.Id == SendingGroupId)
                .Include(x => x.Outboxes)
                .Include(x => x.Templates)
                .FirstOrDefaultAsync();

            if (sendingGroup == null)
            {
                _logger.Error($"发件组 {SendingGroupId} 不存在");
                return false;
            }
            // 保存发件组
            _sendingGroup = sendingGroup;

            // 将收件箱重置为空，方便垃圾回收
            sendingGroup.Inboxes = [];

            // 更新用户 id
            UserId = sendingGroup.UserId;

            // 将公共的发件箱添加到发件池中
            // 邮件级别的发件箱在初始化发送项时，再添加
            await AddSharedOutboxToPool(
                sendingContext,
                sendingGroup.Outboxes,
                sendingGroup.OutboxGroups
            );

            // 保存所使用的代理
            ProxyIds = sendingGroup.ProxyIds ?? [];

            // 更新代理缓存
            var proxyManager = sendingContext.Provider.GetRequiredService<ProxiesManager>();
            await proxyManager.UpdateUserProxies(sendingContext.Provider, UserId);

            // 获取所有的模板，模板是用户级别的
            _usableTemplates = new UsableTemplateList(UserId);
            // 添加组的通用模板
            _usableTemplates.AddSendingGroupTemplates(
                _sendingGroup.Templates!.ConvertAll(x => x.Id)
            );

            return true;
        }

        /// <summary>
        /// 将新的发件箱添加到发件池中
        /// 会自动去重
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="outboxes"></param>
        /// <param name="outboxGroup"></param>
        /// <returns></returns>
        private async Task AddSharedOutboxToPool(
            SendingContext sendingContext,
            List<Outbox>? outboxes,
            List<IdAndName>? outboxGroup
        )
        {
            outboxes ??= [];
            if (outboxes.Count == 0 && (outboxGroup == null || outboxGroup.Count == 0))
                return;
            var container = sendingContext.Provider.GetRequiredService<OutboxesManager>();

            var outboxAddresses = outboxes.ConvertAll(x => new OutboxEmailAddress(
                x,
                SendingGroupId,
                SmtpPasswordSecretKeys,
                OutboxEmailAddressType.Shared
            ));
            foreach (var outbox in outboxAddresses)
            {
                container.AddOutbox(outbox);
            }

            // 解析发件箱组
            if (outboxGroup == null || outboxGroup.Count == 0)
                return;
            var outboxGroupIds = outboxGroup.Select(x => x.Id).ToList();
            // 添加发件组的发件箱
            var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();
            var groupBoxes = await sqlContext
                .Outboxes.AsNoTracking()
                .Where(x => outboxGroupIds.Contains(x.EmailGroupId))
                .ToListAsync();
            await AddSharedOutboxToPool(sendingContext, groupBoxes, null);
        }

        /// <summary>
        /// 初始化发件组的发件项
        /// 允许被被多次调用，该接口会查找 SendingItemStatus.CanSend 的项
        /// 多次调用的场景: 重发部分发件项
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="sendingItemIds">只发送特定的发件项</param>
        /// <returns></returns>
        public async Task<bool> InitSendingItems(
            SendingContext sendingContext,
            List<long>? sendingItemIds
        )
        {
            await _initLock.WaitAsync();
            try
            {
                var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();
                if (_reader == null)
                {
                    _reader = readerPool.Open(
                        SendingGroupId,
                        sendingItemIds,
                        includePending: _sendingGroup.Status == SendingGroupStatus.Sending
                    );
                    await LoadNextPage(sendingContext);
                }
                else if (sendingItemIds is { Count: > 0 })
                {
                    // 运行中的部分重发来自显式 ID，本身已在请求内存中，按页读取即可。
                    foreach (var ids in sendingItemIds.Distinct().Chunk(100))
                    {
                        var descriptors = await sqlContext
                            .SendingItems.AsNoTracking()
                            .Where(x => x.SendingGroupId == SendingGroupId && ids.Contains(x.Id))
                            .Where(x =>
                                x.Status == SendingItemStatus.Created
                                || x.Status == SendingItemStatus.Failed
                            )
                            .OrderBy(x => x.OutBoxId)
                            .ThenBy(x => x.Id)
                            .Select(x => new SendItemDescriptor(
                                x.Id,
                                x.SendingGroupId,
                                x.OutBoxId,
                                x.TriedCount
                            ))
                            .ToListAsync();
                        await RegisterDescriptors(sendingContext, descriptors);
                    }
                }

                // 更新当前发件组的数据
                await UpdateSendingGroupInfo(sqlContext, SendingGroupId);

                // 通知用户，任务已开始
                if (!_startNotified)
                {
                    _startNotified = true;
                    await sendingContext
                        .HubClient.GetUserClient(UserId)
                        .SendingGroupProgressChanged(
                            new SendingGroupProgressArg(_sendingGroup, _startDate)
                            {
                                ProgressType = ProgressType.Start
                            }
                        );
                }

                return true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private async Task LoadNextPage(SendingContext sendingContext)
        {
            if (_reader == null || _reader.IsCompleted)
                return;

            await _reader.EnsureBufferedAsync();
            var descriptors = new List<SendItemDescriptor>();
            while (_reader.TryRead(out var descriptor))
                descriptors.Add(descriptor!);

            await RegisterDescriptors(sendingContext, descriptors);
        }

        private async Task RegisterDescriptors(
            SendingContext sendingContext,
            IReadOnlyList<SendItemDescriptor> descriptors
        )
        {
            if (descriptors.Count == 0)
                return;

            var existingIds = _sendingItemMetas.SendingItemIds.ToHashSet();
            var pending = descriptors.Where(x => !existingIds.Contains(x.Id)).ToList();
            if (pending.Count == 0)
                return;

            var sqlContext = sendingContext.SqlContext;
            var specificOutboxIds = pending
                .Where(x => x.OutboxId > 0)
                .Select(x => x.OutboxId)
                .Distinct()
                .ToList();
            var outboxes = await sqlContext
                .Outboxes.AsNoTracking()
                .Where(x => specificOutboxIds.Contains(x.Id))
                .ToListAsync();
            var validOutboxIds = outboxes.Select(x => x.Id).ToHashSet();
            var invalidIds = pending
                .Where(x => x.OutboxId > 0 && !validOutboxIds.Contains(x.OutboxId))
                .Select(x => x.Id)
                .ToList();
            if (invalidIds.Count > 0)
            {
                await sqlContext.SendingItems.UpdateAsync(
                    x => invalidIds.Contains(x.Id),
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Invalid)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(y => y.SendResult, "指定的发件箱已被删除")
                );
                pending.RemoveAll(x => invalidIds.Contains(x.Id));
            }

            _sendingItemMetas.AddRange(
                pending.Select(x => new SendItemMeta(x.Id, x.OutboxId, x.TriedCount))
            );

            var outboxesPool = sendingContext.Provider.GetRequiredService<OutboxesManager>();
            foreach (var outbox in outboxes)
            {
                var itemIds = pending
                    .Where(x => x.OutboxId == outbox.Id)
                    .Select(x => x.Id)
                    .ToList();
                if (itemIds.Count == 0)
                    continue;
                outboxesPool.AddOutbox(
                    new OutboxEmailAddress(
                        outbox,
                        SendingGroupId,
                        SmtpPasswordSecretKeys,
                        OutboxEmailAddressType.Specific,
                        itemIds
                    )
                );
            }

            var pendingIds = pending.Select(x => x.Id).ToList();
            if (pendingIds.Count > 0)
            {
                await sqlContext.SendingItems.UpdateAsync(
                    x => pendingIds.Contains(x.Id),
                    x => x.SetProperty(y => y.Status, SendingItemStatus.Pending)
                );
            }
        }

        /// <summary>
        /// 更新发件组信息，并保存到数据库中
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="sendingGroupId"></param>
        /// <returns></returns>
        private async Task UpdateSendingGroupInfo(SqlContext sqlContext, long sendingGroupId)
        {
            // 获取成功数、失败数、总数
            var sendingGroup = await sqlContext
                .SendingGroups.Where(x => x.Id == sendingGroupId)
                .FirstOrDefaultAsync();
            if (sendingGroup == null)
                return;

            // 更新发件组的信息
            sendingGroup.Status = SendingGroupStatus.Sending;
            var items = sqlContext
                .SendingItems.AsNoTracking()
                .Where(x => x.SendingGroupId == sendingGroup.Id);
            sendingGroup.TotalCount = await items.CountAsync();
            sendingGroup.SuccessCount = await items.CountAsync(x =>
                x.Status >= SendingItemStatus.Success
            );
            // 有发送日期的项，表示已经发送过了
            sendingGroup.SentCount =
                await items.CountAsync(x => x.Status <= SendingItemStatus.Cancel)
                + sendingGroup.SuccessCount;
            // 开始发送日期
            if (sendingGroup.SendStartDate == DateTime.MinValue)
                sendingGroup.SendStartDate = DateTime.UtcNow;
            // 保存组状态
            await sqlContext.SaveChangesAsync();

            // 更新到当前类中
            _sendingGroup.Status = sendingGroup.Status;
            _sendingGroup.TotalCount = sendingGroup.TotalCount;
            _sendingGroup.SuccessCount = sendingGroup.SuccessCount;
            _sendingGroup.SentCount = sendingGroup.SentCount;
            _sendingGroup.SendStartDate = sendingGroup.SendStartDate;
        }

        /// <summary>
        /// 获取发件项
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemMeta?> GetEmailItem(SendingContext sendingContext)
        {
            // 保存当前组的开始日期
            sendingContext.GroupTaskStartDate = _startDate;
            sendingContext.GroupTask = this;

            var outbox = sendingContext.OutboxAddress;
            if (outbox == null)
                return null;

            // 判断是否为当前组对应的发件箱
            if (!outbox.ContainsSendingGroup(SendingGroupId))
                return null;

            // 从列表中移除发件项并转换成 sendItem
            var sendItemMeta = await GetEmailItemFromDb(sendingContext);
            if (sendItemMeta == null)
            {
                await LoadNextPage(sendingContext);
                sendItemMeta = await GetEmailItemFromDb(sendingContext);
                if (sendItemMeta == null)
                    return null;
            }

            // 当前租约进入回收站后预取下一页，避免最后一项提交时误判组已结束。
            if (_sendingItemMetas.WaitListCount < 50)
                await LoadNextPage(sendingContext);

            // 为 sendItem 动态赋值
            // 赋予发件箱
            sendItemMeta.SetOutbox(outbox);

            var sendingSetting = await sendingContext
                .Provider.GetRequiredService<AppSettingsManager>()
                .GetSetting<SendingSetting>(
                    sendingContext.SqlContext,
                    sendItemMeta.SendingItem.UserId
                );
            sendItemMeta.SetReplyToEmails(outbox.ReplyToEmails, sendingSetting.ReplyToEmailsList);

            // 推送开始发件
            await sendingContext
                .HubClient.GetUserClient(UserId)
                .SendingItemStatusChanged(
                    new SendingItemStatusChangedArg(sendItemMeta.SendingItem)
                    {
                        Status = SendingItemStatus.Sending
                    }
                );

            return sendItemMeta;
        }

        /// <summary>
        /// 从数据库中获取发件项
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<SendItemMeta?> GetEmailItemFromDb(SendingContext sendingContext)
        {
            var outbox =
                sendingContext.OutboxAddress ?? throw new Exception("GetSendItem 调用失败, 请先获取发件箱");

            // 先发指定项
            SendItemMeta? sendItemMeta = null;
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Specific))
            {
                // 获取特定项
                sendItemMeta = _sendingItemMetas.GetSendingMeta(outbox.Id);
            }

            // 若特定项已经发完，则从共享项中获取
            if (sendItemMeta == null && outbox.Type.HasFlag(OutboxEmailAddressType.Shared))
            {
                sendItemMeta = _sendingItemMetas.GetSendingMeta();
            }
            if (sendItemMeta == null)
                return null;

            if (sendItemMeta.Lease is null)
            {
                var acquired = leaseStore.TryAcquire(
                    new SendItemDescriptor(
                        sendItemMeta.SendingItemId,
                        SendingGroupId,
                        sendItemMeta.OutboxId,
                        sendItemMeta.TriedCount
                    ),
                    new OutboxKey(outbox.UserId, outbox.Id),
                    timeProvider.GetUtcNow(),
                    quotaOptions.Value.LeaseDuration,
                    out var lease
                );
                if (!acquired)
                {
                    _sendingItemMetas.Release(sendItemMeta);
                    return null;
                }
                sendItemMeta.SetLease(lease);
            }

            // 如果已经包含 SendingItem, 说明初始化过了，直接返回
            if (sendItemMeta.Initialized)
            {
                return sendItemMeta;
            }
            sendItemMeta.Initialized = true;

            // 拉取发件项
            var sendingItem = await payloadReader.ReadAsync(
                sendingContext.SqlContext,
                sendItemMeta.SendingItemId
            );
            if (sendingItem == null)
            {
                sendItemMeta.SetStatus(SendItemMetaStatus.Error, "发件项不存在或已被删除");
                CompleteEmailItem(sendItemMeta);
                return null;
            }
            sendItemMeta.SetSendingItem(sendingItem);

            _usableTemplates.AddSendingItemTemplate(
                sendItemMeta.SendingItem.Id,
                sendItemMeta.SendingItem.EmailTemplateId
            );

            var filters = sendingContext.Provider.GetServices<ISendingItemFilter>();
            foreach (var filter in filters)
            {
                var invalidIds = await filter.GetInvalidSendingItemIds([sendItemMeta.SendingItem]);
                if (!invalidIds.Contains(sendItemMeta.SendingItemId))
                    continue;

                await sendingContext.SqlContext.SendingItems.UpdateAsync(
                    x => x.Id == sendItemMeta.SendingItemId,
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Invalid)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(y => y.SendResult, "发件项过滤器判定为无效")
                );
                sendItemMeta.SetStatus(SendItemMetaStatus.Error, "发件项过滤器判定为无效");
                CompleteEmailItem(sendItemMeta);
                return null;
            }

            var preparer = sendingContext.Provider.GetRequiredService<ISendItemPreparer>();
            await preparer.Prepare(
                sendingContext,
                sendItemMeta,
                _sendingGroup,
                _usableTemplates,
                ProxyIds
            );

            return sendItemMeta;
        }

        /// <summary>
        /// 判断 outbox 能否匹配到发件项
        /// </summary>
        /// <param name="outbox"></param>
        /// <returns></returns>
        public bool MatchEmailItem(OutboxEmailAddress outbox)
        {
            // 特定发件箱
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Specific))
            {
                var matchSpecific = _sendingItemMetas.MatchSendingMeta(outbox.Id, true);
                if (matchSpecific)
                    return true;
                if (_delayedItems.Values.Any(x => x.OutboxId == outbox.Id))
                    return true;
            }

            // 从当前组中获取
            if (outbox.Type.HasFlag(OutboxEmailAddressType.Shared))
            {
                var matchShared = _sendingItemMetas.MatchSendingMeta(outbox.Id, false);
                if (matchShared)
                    return true;
                if (_delayedItems.Values.Any(x => x.OutboxId <= 0))
                    return true;
            }

            if (_reader is { IsCompleted: false })
                return true;

            return false;
        }

        public bool MatchReadyEmailItem(OutboxEmailAddress outbox)
        {
            if (
                outbox.Type.HasFlag(OutboxEmailAddressType.Specific)
                && _sendingItemMetas.MatchReadyMeta(outbox.Id, true)
            )
                return true;
            if (
                outbox.Type.HasFlag(OutboxEmailAddressType.Shared)
                && _sendingItemMetas.MatchReadyMeta(outbox.Id, false)
            )
                return true;
            return _reader is { IsCompleted: false };
        }

        /// <summary>
        /// 移除指定发件箱的发件项
        /// </summary>
        /// <param name="outboxEmail"></param>
        /// <returns></returns>
        public void RemovePendingItems(List<long> sendingItemIds)
        {
            sendingItemIds.ForEach(x => _sendingItemMetas.RemovePendingItem(x));
        }

        public bool CompleteEmailItem(SendItemMeta item)
        {
            CompleteLease(item);
            return _sendingItemMetas.Complete(item);
        }

        public bool ScheduleRetryEmailItem(SendItemMeta item, DateTimeOffset retryAt)
        {
            CompleteLease(item);
            if (!_sendingItemMetas.Complete(item))
                return false;

            item.IncreaseTriedCount();
            var delayed = new DelayedItem(item.OutboxId, item.TriedCount);
            if (!_delayedItems.TryAdd(item.SendingItemId, delayed))
            {
                _sendingItemMetas.Add(
                    new SendItemMeta(item.SendingItemId, item.OutboxId, item.TriedCount)
                );
                return false;
            }

            delayed.Task = RequeueAfterDelayAsync(item.SendingItemId, delayed, retryAt);
            return true;
        }

        public bool ReleaseEmailItem(SendItemMeta item)
        {
            if (item.Lease is not null)
                leaseStore.TryRevoke(item.Lease.LeaseId, timeProvider.GetUtcNow(), out _);
            return _sendingItemMetas.Release(item);
        }

        public void Close()
        {
            if (Interlocked.Exchange(ref _closed, 1) != 0)
                return;
            _lifetime.Cancel();
            foreach (var item in _sendingItemMetas.GetActiveItems())
            {
                if (item.Lease is not null)
                    leaseStore.TryRevoke(item.Lease.LeaseId, timeProvider.GetUtcNow(), out _);
            }
            readerPool.Close(SendingGroupId);
            _reader = null;
        }

        private async Task RequeueAfterDelayAsync(
            long sendingItemId,
            DelayedItem delayed,
            DateTimeOffset retryAt
        )
        {
            try
            {
                var delay = retryAt - timeProvider.GetUtcNow();
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, timeProvider, _lifetime.Token);
                if (Volatile.Read(ref _closed) != 0)
                    return;

                _sendingItemMetas.Add(
                    new SendItemMeta(sendingItemId, delayed.OutboxId, delayed.TriedCount)
                );
                await workerCoordinator.StartSendingAsync(_lifetime.Token);
            }
            catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
            catch (Exception exception)
            {
                _logger.Error($"发件项 {sendingItemId} 重新入队失败", exception);
            }
            finally
            {
                _delayedItems.TryRemove(sendingItemId, out _);
            }
        }

        private void CompleteLease(SendItemMeta item)
        {
            if (item.Lease is null)
                return;
            if (!leaseStore.TryComplete(item.Lease.LeaseId, timeProvider.GetUtcNow(), out _))
                _logger.Warn($"发件项 {item.SendingItemId} 的租约已过期或完成");
        }
    }
}
