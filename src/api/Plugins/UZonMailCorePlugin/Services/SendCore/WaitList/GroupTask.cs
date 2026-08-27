using System.Collections.Concurrent;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Reading;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
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
        SenderAccountRuntimeFactory senderAccountRuntimeFactory,
        ISendItemReaderPool readerPool,
        ISendPayloadReader payloadReader,
        ISendLeaseStore leaseStore,
        ISendingWorkerCoordinator workerCoordinator,
        EmailTemplateCacheLeaseManager templateCacheLeaseManager,
        IOptions<SendingQuotaOptions> quotaOptions,
        IOptions<SenderAccountSupplyOptions> senderAccountSupplyOptions,
        TimeProvider timeProvider
    ) : ITransientService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupTask));
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private SendItemReaderSession? _reader;
        private bool _startNotified;
        private readonly ConcurrentDictionary<long, DelayedItem> _delayedItems = [];
        private readonly ConcurrentDictionary<long, SendLease> _activeLeases = [];
        private readonly CancellationTokenSource _lifetime = new();
        private int _closed;
        private readonly SemaphoreSlim _senderAccountCatalogLock = new(1, 1);
        private List<long> _senderAccountGroupIds = [];
        private long _sharedSenderAccountCursor;
        private bool _isSharedSenderAccountCatalogCompleted;

        private sealed class DelayedItem(SendItemDescriptor descriptor)
        {
            public SendItemDescriptor Descriptor { get; } = descriptor;
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
            try
            {
                // 初始化组
                if (await groupTask.InitSendingGroup(ctx))
                    return groupTask;

                await groupTask.CloseAsync();
                return null;
            }
            catch
            {
                await groupTask.CloseAsync();
                throw;
            }
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
        /// 所属用户
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 发件项数据
        /// 此处只保存自由发件的项
        /// 若指定发件箱，数据 id 会保存在 senderAccount 中
        /// </summary>
        private readonly SendItemQueue _sendItemQueue = new();

        /// <summary>
        /// 可用的代理
        /// </summary>
        private List<long> ProxyIds { get; set; } = [];

        /// <summary>
        /// 发件组模板解析器
        /// </summary>
        private SendingGroupTemplateResolver? _templateResolver;

        /// <summary>
        /// 是否应该释放
        /// </summary>
        public bool ShouldDispose =>
            _sendItemQueue.Count == 0
            && _delayedItems.IsEmpty
            && _reader is not { IsCompleted: false };

        public int ReadyCount => _sendItemQueue.ReadyCount;

        public int ActiveCount => _sendItemQueue.ActiveCount;

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
            sendingGroup.Recipients = [];

            // 更新用户 id
            UserId = sendingGroup.UserId;

            // 将公共的发件箱添加到发件池中
            // 邮件级别的发件箱在初始化发送项时，再添加
            _senderAccountGroupIds =
                sendingGroup.SenderAccountGroups?.Select(group => group.Id).ToList() ?? [];
            await LoadNextSharedSenderAccountPage(sendingContext);

            // 保存所使用的代理
            ProxyIds = sendingGroup.ProxyIds ?? [];

            // 更新代理缓存
            var proxyManager = sendingContext.Provider.GetRequiredService<IProxiesManager>();
            await proxyManager.UpdateUserProxies(sendingContext.Provider, UserId);

            // 模板内容由全局缓存管理，当前任务只保存选择规则和租约。
            _templateResolver = new SendingGroupTemplateResolver(UserId, templateCacheLeaseManager);
            // 添加组的通用模板
            _templateResolver.AddSendingGroupTemplates(
                _sendingGroup.Templates!.ConvertAll(x => x.Id)
            );

            return true;
        }

        /// <summary>
        /// 将新的发件箱添加到发件池中
        /// 会自动去重
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="senderAccounts"></param>
        /// <param name="senderAccountGroup"></param>
        /// <returns></returns>
        /// <summary>
        /// 从共享发件箱目录继续读取一页，并返回实际注册数量。
        /// </summary>
        public async Task<int> LoadNextSharedSenderAccountPage(
            SendingContext sendingContext,
            int maxCount = int.MaxValue
        )
        {
            if (_isSharedSenderAccountCatalogCompleted)
                return 0;

            await _senderAccountCatalogLock.WaitAsync(_lifetime.Token);
            try
            {
                if (_isSharedSenderAccountCatalogCompleted)
                    return 0;

                var sqlContext = sendingContext.Provider.GetRequiredService<SqlContext>();
                var directlyLinkedSenderAccounts = sqlContext
                    .SendingGroups.AsNoTracking()
                    .Where(group => group.Id == SendingGroupId)
                    .SelectMany(group => group.SenderAccounts);
                var groupedSenderAccounts = sqlContext
                    .SenderAccounts.AsNoTracking()
                    .Where(senderAccount =>
                        _senderAccountGroupIds.Contains(senderAccount.EmailGroupId)
                    );
                var pageSize = Math.Min(senderAccountSupplyOptions.Value.CatalogPageSize, maxCount);
                if (pageSize <= 0)
                    return 0;
                var senderAccounts = await directlyLinkedSenderAccounts
                    .Union(groupedSenderAccounts)
                    .Where(senderAccount =>
                        senderAccount.Id > _sharedSenderAccountCursor
                        && senderAccount.Status == SenderAccountStatus.Valid
                    )
                    .OrderBy(senderAccount => senderAccount.Id)
                    .Take(pageSize)
                    .Include(senderAccount => senderAccount.EmailAccount)
                    .ThenInclude(emailAccount => emailAccount.OAuthCredential)
                    .Include(senderAccount => senderAccount.SmtpCredential)
                    .ToListAsync(_lifetime.Token);

                if (senderAccounts.Count > 0)
                    _sharedSenderAccountCursor = senderAccounts[^1].Id;
                _isSharedSenderAccountCatalogCompleted = senderAccounts.Count < pageSize;

                var container = sendingContext.Provider.GetRequiredService<SenderAccountsManager>();
                foreach (var senderAccount in senderAccounts)
                {
                    container.AddSenderAccount(
                        senderAccountRuntimeFactory.Create(
                            senderAccount,
                            SendingGroupId,
                            SenderEmailAddressType.Shared
                        )
                    );
                }

                return senderAccounts.Count;
            }
            finally
            {
                _senderAccountCatalogLock.Release();
            }
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
                        includePending: _sendingGroup.Status
                            is SendingGroupStatus.Sending
                                or SendingGroupStatus.WaitingForQuotaReset
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
                            .ExcludeHardBounceItems()
                            .Where(x =>
                                x.Status == SendingItemStatus.Created
                                || x.Status == SendingItemStatus.Failed
                            )
                            .OrderBy(x => x.SenderAccountId)
                            .ThenBy(x => x.Id)
                            .Select(x => new SendItemDescriptor(
                                x.Id,
                                x.SendingGroupId,
                                x.SenderAccountId,
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

            var existingIds = _sendItemQueue.SendingItemIds.ToHashSet();
            var pending = descriptors.Where(x => !existingIds.Contains(x.Id)).ToList();
            if (pending.Count == 0)
                return;

            var sqlContext = sendingContext.SqlContext;
            var specificSenderAccountIds = pending
                .Where(x => x.SenderAccountId > 0)
                .Select(x => x.SenderAccountId)
                .Distinct()
                .ToList();
            var senderAccounts = await sqlContext
                .SenderAccounts.AsNoTracking()
                .Where(x => specificSenderAccountIds.Contains(x.Id))
                .Include(x => x.EmailAccount)
                .ThenInclude(x => x.OAuthCredential)
                .Include(x => x.SmtpCredential)
                .ToListAsync();
            var validSenderAccountIds = senderAccounts.Select(x => x.Id).ToHashSet();
            var invalidIds = pending
                .Where(x =>
                    x.SenderAccountId > 0 && !validSenderAccountIds.Contains(x.SenderAccountId)
                )
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

            _sendItemQueue.AddRange(pending);

            var senderAccountsPool =
                sendingContext.Provider.GetRequiredService<SenderAccountsManager>();
            foreach (var senderAccount in senderAccounts)
            {
                var itemIds = pending
                    .Where(x => x.SenderAccountId == senderAccount.Id)
                    .Select(x => x.Id)
                    .ToList();
                if (itemIds.Count == 0)
                    continue;
                senderAccountsPool.AddSenderAccount(
                    senderAccountRuntimeFactory.Create(
                        senderAccount,
                        SendingGroupId,
                        SenderEmailAddressType.Specific,
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
        public async Task<SendItemExecution?> GetEmailItem(SendingContext sendingContext)
        {
            // 保存当前组的开始日期
            sendingContext.GroupTaskStartDate = _startDate;
            sendingContext.GroupTask = this;

            var senderAccount = sendingContext.SenderAccountAddress;
            if (senderAccount == null)
                return null;

            // 判断是否为当前组对应的发件箱
            if (!senderAccount.ContainsSendingGroup(SendingGroupId))
                return null;

            // 从列表中移除发件项并转换成 sendItem
            var execution = await GetEmailItemFromDb(sendingContext);
            if (execution == null)
            {
                await LoadNextPage(sendingContext);
                execution = await GetEmailItemFromDb(sendingContext);
                if (execution == null)
                    return null;
            }

            // 当前租约进入回收站后预取下一页，避免最后一项提交时误判组已结束。
            if (_sendItemQueue.ReadyCount < 50)
                await LoadNextPage(sendingContext);

            // 推送开始发件
            await sendingContext
                .HubClient.GetUserClient(UserId)
                .SendingItemStatusChanged(
                    new SendingItemStatusChangedArg(execution.PreparedItem.SourceItem)
                    {
                        Status = SendingItemStatus.Sending
                    }
                );

            return execution;
        }

        /// <summary>
        /// 从数据库中获取发件项
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <returns></returns>
        private async Task<SendItemExecution?> GetEmailItemFromDb(SendingContext sendingContext)
        {
            var senderAccount =
                sendingContext.SenderAccountAddress
                ?? throw new Exception("GetSendItem 调用失败, 请先获取发件箱");

            // 先发指定项
            SendItemDescriptor? descriptor = null;
            var bindingType = senderAccount.GetTypeForSendingGroup(SendingGroupId);
            if (bindingType.HasFlag(SenderEmailAddressType.Specific))
            {
                // 获取特定项
                descriptor = _sendItemQueue.AcquireSpecific(senderAccount.Id);
            }

            // 若特定项已经发完，则从共享项中获取
            if (descriptor == null && bindingType.HasFlag(SenderEmailAddressType.Shared))
            {
                descriptor = _sendItemQueue.AcquireShared();
            }
            if (descriptor == null)
                return null;

            var acquired = leaseStore.TryAcquire(
                descriptor,
                new SenderAccountKey(senderAccount.UserId, senderAccount.Id),
                timeProvider.GetUtcNow(),
                quotaOptions.Value.LeaseDuration,
                out var lease
            );
            if (!acquired)
            {
                _sendItemQueue.Release(descriptor);
                return null;
            }
            _activeLeases[descriptor.Id] = lease;

            // 拉取发件项
            var sendingItem = await payloadReader.ReadAsync(
                sendingContext.SqlContext,
                descriptor.Id
            );
            if (sendingItem == null)
            {
                CompleteAcquisition(descriptor, lease);
                return null;
            }

            _templateResolver?.AddSendingItemTemplate(sendingItem.Id, sendingItem.EmailTemplateId);

            var filters = sendingContext.Provider.GetServices<ISendingItemFilter>();
            foreach (var filter in filters)
            {
                var invalidIds = await filter.GetInvalidSendingItemIds([sendingItem]);
                if (!invalidIds.Contains(descriptor.Id))
                    continue;

                await sendingContext.SqlContext.SendingItems.UpdateAsync(
                    x => x.Id == descriptor.Id,
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Invalid)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(y => y.SendResult, "发件项过滤器判定为无效")
                );
                CompleteAcquisition(descriptor, lease);
                return null;
            }

            var preparer = sendingContext.Provider.GetRequiredService<ISendItemPreparer>();
            var preparedItem = await preparer.PrepareAsync(
                sendingContext,
                sendingItem,
                senderAccount,
                _sendingGroup,
                _templateResolver ?? throw new InvalidOperationException("发件组模板解析器尚未初始化"),
                ProxyIds
            );

            return new SendItemExecution(lease, preparedItem);
        }

        /// <summary>
        /// 判断 senderAccount 能否匹配到发件项
        /// </summary>
        /// <param name="senderAccount"></param>
        /// <returns></returns>
        public bool MatchEmailItem(SenderEmailAddress senderAccount)
        {
            // 特定发件箱
            var bindingType = senderAccount.GetTypeForSendingGroup(SendingGroupId);
            if (bindingType.HasFlag(SenderEmailAddressType.Specific))
            {
                var matchSpecific = _sendItemQueue.Contains(senderAccount.Id, true);
                if (matchSpecific)
                    return true;
                if (_delayedItems.Values.Any(x => x.Descriptor.SenderAccountId == senderAccount.Id))
                    return true;
            }

            // 从当前组中获取
            if (bindingType.HasFlag(SenderEmailAddressType.Shared))
            {
                var matchShared = _sendItemQueue.Contains(senderAccount.Id, false);
                if (matchShared)
                    return true;
                if (_delayedItems.Values.Any(x => x.Descriptor.SenderAccountId <= 0))
                    return true;
            }

            if (_reader is { IsCompleted: false })
                return true;

            return false;
        }

        public bool MatchReadyEmailItem(SenderEmailAddress senderAccount)
        {
            if (
                senderAccount
                    .GetTypeForSendingGroup(SendingGroupId)
                    .HasFlag(SenderEmailAddressType.Specific)
                && _sendItemQueue.ContainsReady(senderAccount.Id, true)
            )
                return true;
            if (
                senderAccount
                    .GetTypeForSendingGroup(SendingGroupId)
                    .HasFlag(SenderEmailAddressType.Shared)
                && _sendItemQueue.ContainsReady(senderAccount.Id, false)
            )
                return true;
            return _reader is { IsCompleted: false };
        }

        /// <summary>
        /// 移除指定发件箱的发件项
        /// </summary>
        /// <param name="senderAccountEmail"></param>
        /// <returns></returns>
        public void RemovePendingItems(List<long> sendingItemIds)
        {
            sendingItemIds.ForEach(x => _sendItemQueue.RemovePendingItem(x));
        }

        /// <summary>提交租约并从队列移除已结束的发件项。</summary>
        public bool CompleteEmailItem(SendItemExecution execution)
        {
            CompleteLease(execution.Lease);
            return _sendItemQueue.Complete(execution.Descriptor);
        }

        /// <summary>提交当前租约，并以递增后的重试次数延迟重新入队。</summary>
        public bool ScheduleRetryEmailItem(SendItemExecution execution, DateTimeOffset retryAt)
        {
            CompleteLease(execution.Lease);
            if (!_sendItemQueue.Complete(execution.Descriptor))
                return false;

            var retryDescriptor = execution.Descriptor with
            {
                TriedCount = execution.Descriptor.TriedCount + 1,
            };
            var delayed = new DelayedItem(retryDescriptor);
            if (!_delayedItems.TryAdd(retryDescriptor.Id, delayed))
            {
                _sendItemQueue.Add(retryDescriptor);
                return false;
            }

            delayed.Task = RequeueAfterDelayAsync(retryDescriptor.Id, delayed, retryAt);
            return true;
        }

        /// <summary>撤销当前租约并原样释放回队列，不消耗重试次数。</summary>
        public bool ReleaseEmailItem(SendItemExecution execution)
        {
            _activeLeases.TryRemove(execution.Descriptor.Id, out _);
            leaseStore.TryRevoke(execution.Lease.LeaseId, timeProvider.GetUtcNow(), out _);
            return _sendItemQueue.Release(execution.Descriptor);
        }

        /// <summary>
        /// 关闭组任务并释放模板与发件项租约。
        /// 所有终止路径共用此入口，避免发件组移除后仍保留模板缓存引用。
        /// </summary>
        public async Task CloseAsync()
        {
            if (Interlocked.Exchange(ref _closed, 1) != 0)
                return;

            _lifetime.Cancel();
            if (_templateResolver != null)
                await _templateResolver.DisposeAsync();
            foreach (var lease in _activeLeases.Values)
            {
                leaseStore.TryRevoke(lease.LeaseId, timeProvider.GetUtcNow(), out _);
            }
            _activeLeases.Clear();
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

                _sendItemQueue.Add(delayed.Descriptor);
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

        private void CompleteAcquisition(SendItemDescriptor descriptor, SendLease lease)
        {
            CompleteLease(lease);
            _sendItemQueue.Complete(descriptor);
        }

        private void CompleteLease(SendLease lease)
        {
            _activeLeases.TryRemove(lease.Item.Id, out _);
            if (!leaseStore.TryComplete(lease.LeaseId, timeProvider.GetUtcNow(), out _))
                _logger.Warn($"发件项 {lease.Item.Id} 的租约已过期或完成");
        }
    }
}
