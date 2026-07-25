using System.Collections.Concurrent;
using System.Threading.Channels;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore;

public sealed class SendingTasksManager
    : ISendingTasksManager,
        ISendingWorkerCoordinator,
        ISingletonService<ISendingTasksManager>,
        ISingletonService<ISendingWorkerCoordinator>,
        IAsyncDisposable
{
    private sealed record WorkerInfo(long OrganizationId, long UserId, Task Task);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(SendingTasksManager));
    private readonly IServiceProvider _provider;
    private readonly OutboxesManager _outboxesManager;
    private readonly UserGroupTasksPools _userGroupTasksPools;
    private readonly SendingQuotaOptions _quotas;
    private readonly OutboxSupplyOptions _outboxSupply;
    private readonly TimeProvider _timeProvider;
    private readonly FairSendingTaskSelector _taskSelector = new();
    private readonly ConcurrentDictionary<OutboxKey, WorkerInfo> _workers = [];
    private readonly ConcurrentDictionary<long, long> _userOrganizations = [];
    private readonly Channel<bool> _wakeUps = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        }
    );
    private readonly CancellationTokenSource _shutdown = new();
    private readonly object _scheduledWakeLock = new();
    private CancellationTokenSource? _scheduledWake;
    private long _scheduledWakeAtUtcTicks;
    private int _isRefillingOutboxes;
    private int _refillStartOffset;
    private readonly Task _dispatcher;

    public SendingTasksManager(
        IServiceProvider provider,
        OutboxesManager outboxesManager,
        UserGroupTasksPools userGroupTasksPools,
        IOptions<SendingQuotaOptions> quotas,
        IOptions<OutboxSupplyOptions> outboxSupply,
        TimeProvider timeProvider
    )
    {
        _provider = provider;
        _outboxesManager = outboxesManager;
        _userGroupTasksPools = userGroupTasksPools;
        _quotas = quotas.Value;
        _outboxSupply = outboxSupply.Value;
        _timeProvider = timeProvider;
        _quotas.Validate();
        _outboxSupply.Validate();
        _dispatcher = DispatchLoopAsync(_shutdown.Token);
    }

    public int RunningTasksCount => _workers.Count;

    public void RegisterTenant(long userId, long organizationId)
    {
        _userOrganizations[userId] = organizationId;
    }

    public Task StartSendingAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _wakeUps.Writer.TryWrite(true);
        return Task.CompletedTask;
    }

    private async Task DispatchLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _wakeUps.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_wakeUps.Reader.TryRead(out _)) { }
                DispatchAvailableWorkers(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            _logger.Error("发件调度器异常退出", exception);
        }
    }

    private void DispatchAvailableWorkers(CancellationToken cancellationToken)
    {
        var workerSnapshot = _workers.ToArray();
        var maxSelections = _quotas.SystemHardLimit - workerSnapshot.Length;
        if (maxSelections <= 0)
            return;

        var activeKeys = workerSnapshot.Select(pair => pair.Key).ToHashSet();
        var outboxSnapshot = _outboxesManager.Values;
        var organizations = new Dictionary<long, long>();
        var availableOutboxes = new Dictionary<OutboxKey, OutboxEmailAddress>(outboxSnapshot.Count);
        var candidates = new List<SendingTaskCandidate>(outboxSnapshot.Count);
        var utcNow = _timeProvider.GetUtcNow();
        DateTimeOffset? earliestCooldownEnd = null;
        foreach (var outbox in outboxSnapshot)
        {
            var key = new OutboxKey(outbox.UserId, outbox.Id);
            if (outbox.ShouldDispose || activeKeys.Contains(key))
                continue;

            if (!outbox.IsEligible(utcNow))
            {
                if (
                    outbox.IsWorking
                    && outbox.NextEligibleUtc > utcNow
                    && (earliestCooldownEnd == null || outbox.NextEligibleUtc < earliestCooldownEnd)
                )
                    earliestCooldownEnd = outbox.NextEligibleUtc;
                continue;
            }

            if (
                !_userGroupTasksPools.TryGetValue(outbox.UserId, out var userTasksPool)
                || !userTasksPool.MatchReadyEmailItem(outbox)
            )
                continue;

            if (!organizations.TryGetValue(outbox.UserId, out var organizationId))
            {
                organizationId = GetOrganizationId(outbox.UserId);
                organizations.Add(outbox.UserId, organizationId);
            }

            availableOutboxes.Add(key, outbox);
            candidates.Add(
                new SendingTaskCandidate(key, organizationId, outbox.UserId, outbox.CreateDate)
            );
        }

        var readyLowWatermark = _quotas.SystemHardLimit * _outboxSupply.ReadyLowWatermarkMultiplier;
        if (candidates.Count < readyLowWatermark)
            StartOutboxRefill(cancellationToken);

        var snapshot = new SendingDispatchSnapshot(
            candidates,
            workerSnapshot
                .Select(pair => new SendingWorkerAllocation(
                    pair.Value.OrganizationId,
                    pair.Value.UserId
                ))
                .ToArray(),
            maxSelections,
            _quotas.OrganizationFairShare,
            _quotas.UserFairShare
        );
        var selectionCycle = _taskSelector.CreateCycle(snapshot);

        while (selectionCycle.TryReserveNext(out var selected))
        {
            if (
                !availableOutboxes.TryGetValue(selected.Key, out var candidate)
                || !candidate.TryMarkTaskRunning()
            )
            {
                selectionCycle.Reject(selected.Key);
                return;
            }

            var startGate = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var task = RunWorkerAsync(selected.Key, candidate, startGate.Task, cancellationToken);
            var worker = new WorkerInfo(selected.OrganizationId, selected.UserId, task);
            if (_workers.TryAdd(selected.Key, worker))
            {
                try
                {
                    selectionCycle.Commit(selected.Key);
                }
                finally
                {
                    startGate.SetResult(true);
                }
                continue;
            }

            candidate.MarkTaskStopped();
            selectionCycle.Reject(selected.Key);
            startGate.SetResult(false);
            return;
        }

        if (earliestCooldownEnd.HasValue)
            ScheduleWake(earliestCooldownEnd.Value, cancellationToken);
    }

    private async Task RunWorkerAsync(
        OutboxKey key,
        OutboxEmailAddress outbox,
        Task<bool> startGate,
        CancellationToken cancellationToken
    )
    {
        if (!await startGate)
            return;
        _logger.Info($"开始执行发件任务: {key} {outbox.Email}");
        try
        {
            // 一个工作槽只执行一次发送尝试，结束后重新参加组织、用户和组公平调度。
            // 冷却资格由调度器管理，因此这里不会因单个发件箱的冷却长期占住槽位。
            await using var scope = _provider.CreateAsyncScope();
            var sendingContext = scope
                .ServiceProvider.GetRequiredService<Contexts.SendingContext>()
                .SetOutbox(outbox);
            var pipeline = scope.ServiceProvider.GetRequiredService<ISendingPipeline>();
            await pipeline.Handle(sendingContext);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            _logger.Error($"发件箱 {key} 发件任务异常终止", exception);
        }
        finally
        {
            outbox.MarkTaskStopped();
            _workers.TryRemove(key, out _);
            _wakeUps.Writer.TryWrite(true);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _wakeUps.Writer.TryComplete();
        await _shutdown.CancelAsync();
        lock (_scheduledWakeLock)
        {
            _scheduledWake?.Cancel();
            _scheduledWake?.Dispose();
            _scheduledWake = null;
        }
        try
        {
            await _dispatcher;
            await Task.WhenAll(_workers.Values.Select(x => x.Task));
        }
        catch (OperationCanceledException) { }
        _shutdown.Dispose();
    }

    private long GetOrganizationId(long userId) =>
        _userOrganizations.TryGetValue(userId, out var organizationId) ? organizationId : 0;

    private void StartOutboxRefill(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isRefillingOutboxes, 1, 0) != 0)
            return;
        _ = RefillOutboxesAsync(cancellationToken);
    }

    private async Task RefillOutboxesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var readyTarget = _quotas.SystemHardLimit * _outboxSupply.ReadyTargetMultiplier;
            while (
                _outboxesManager.Count < _outboxSupply.MaxTrackedOutboxes
                && GetReadyCandidateCount(_timeProvider.GetUtcNow()) < readyTarget
            )
            {
                var addedInRound = 0;
                await using var scope = _provider.CreateAsyncScope();
                var sendingContext =
                    scope.ServiceProvider.GetRequiredService<Contexts.SendingContext>();
                var groupTasks = _userGroupTasksPools
                    .Values.SelectMany(userPool => userPool.GetTasks())
                    .OrderBy(groupTask => groupTask.UserId)
                    .ThenBy(groupTask => groupTask.SendingGroupId)
                    .ToList();
                if (groupTasks.Count == 0)
                    break;

                var startOffset =
                    (uint)Interlocked.Increment(ref _refillStartOffset) % (uint)groupTasks.Count;
                var readyShortage = Math.Max(
                    1,
                    readyTarget - GetReadyCandidateCount(_timeProvider.GetUtcNow())
                );
                var fairBatchSize = Math.Max(
                    1,
                    Math.Min(
                        _outboxSupply.CatalogPageSize,
                        (readyShortage + groupTasks.Count - 1) / groupTasks.Count
                    )
                );
                for (var offset = 0; offset < groupTasks.Count; offset++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var groupTask = groupTasks[
                        (int)((startOffset + (uint)offset) % (uint)groupTasks.Count)
                    ];
                    var remainingCapacity =
                        _outboxSupply.MaxTrackedOutboxes - _outboxesManager.Count;
                    if (remainingCapacity <= 0)
                        break;
                    addedInRound += await groupTask.LoadNextSharedOutboxPage(
                        sendingContext,
                        Math.Min(remainingCapacity, fairBatchSize)
                    );
                    if (GetReadyCandidateCount(_timeProvider.GetUtcNow()) >= readyTarget)
                        break;
                }

                if (addedInRound == 0)
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            _logger.Error("补充发件箱目录失败", exception);
        }
        finally
        {
            Interlocked.Exchange(ref _isRefillingOutboxes, 0);
            _wakeUps.Writer.TryWrite(true);
        }
    }

    private int GetReadyCandidateCount(DateTimeOffset utcNow)
    {
        var readyCount = 0;
        foreach (var outbox in _outboxesManager.Values)
        {
            if (
                outbox.IsEligible(utcNow)
                && _userGroupTasksPools.TryGetValue(outbox.UserId, out var userTasksPool)
                && userTasksPool.MatchReadyEmailItem(outbox)
            )
                readyCount++;
        }
        return readyCount;
    }

    private void ScheduleWake(DateTimeOffset wakeAtUtc, CancellationToken cancellationToken)
    {
        CancellationTokenSource scheduledWake;
        lock (_scheduledWakeLock)
        {
            if (
                _scheduledWake is { IsCancellationRequested: false }
                && _scheduledWakeAtUtcTicks <= wakeAtUtc.UtcTicks
            )
                return;
            _scheduledWake?.Cancel();
            _scheduledWake?.Dispose();
            scheduledWake = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _scheduledWake = scheduledWake;
            _scheduledWakeAtUtcTicks = wakeAtUtc.UtcTicks;
        }

        _ = WakeAtAsync(wakeAtUtc, scheduledWake);
    }

    private async Task WakeAtAsync(DateTimeOffset wakeAtUtc, CancellationTokenSource scheduledWake)
    {
        try
        {
            var delay = wakeAtUtc - _timeProvider.GetUtcNow();
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, _timeProvider, scheduledWake.Token);
            if (wakeAtUtc.UtcDateTime.TimeOfDay == TimeSpan.Zero)
                await ResumeQuotaWaitingGroupsAsync(scheduledWake.Token);
            _wakeUps.Writer.TryWrite(true);
        }
        catch (OperationCanceledException) when (scheduledWake.IsCancellationRequested) { }
        finally
        {
            lock (_scheduledWakeLock)
            {
                if (ReferenceEquals(_scheduledWake, scheduledWake))
                {
                    _scheduledWake.Dispose();
                    _scheduledWake = null;
                    _scheduledWakeAtUtcTicks = 0;
                }
            }
        }
    }

    private async Task ResumeQuotaWaitingGroupsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _provider.CreateAsyncScope();
        var sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        await sqlContext
            .SendingGroups.Where(group =>
                group.Status == SendingGroupStatus.WaitingForQuotaReset
                && group.ResumeAtUtc <= utcNow
            )
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(group => group.Status, SendingGroupStatus.Sending)
                        .SetProperty(group => group.StatusReason, (string?)null)
                        .SetProperty(group => group.ResumeAtUtc, (DateTime?)null),
                cancellationToken
            );
    }
}
