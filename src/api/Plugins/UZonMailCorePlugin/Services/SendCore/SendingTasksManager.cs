using System.Collections.Concurrent;
using System.Threading.Channels;
using log4net;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
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
    private readonly SendingQuotaOptions _quotas;
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
    private readonly Task _dispatcher;

    public SendingTasksManager(
        IServiceProvider provider,
        OutboxesManager outboxesManager,
        IOptions<SendingQuotaOptions> quotas
    )
    {
        _provider = provider;
        _outboxesManager = outboxesManager;
        _quotas = quotas.Value;
        _quotas.Validate();
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
        foreach (var outbox in outboxSnapshot)
        {
            var key = new OutboxKey(outbox.UserId, outbox.Id);
            if (outbox.ShouldDispose || activeKeys.Contains(key))
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
            while (!cancellationToken.IsCancellationRequested && !outbox.ShouldDispose)
            {
                await using var scope = _provider.CreateAsyncScope();
                var sendingContext = scope
                    .ServiceProvider.GetRequiredService<Contexts.SendingContext>()
                    .SetOutbox(outbox);
                var pipeline = scope.ServiceProvider.GetRequiredService<ISendingPipeline>();
                await pipeline.Handle(sendingContext);
                if (sendingContext.ShouldExitTask())
                    break;
                if (sendingContext.CurrentAttempt is null)
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
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
}
