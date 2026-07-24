using System.Collections.Concurrent;
using System.Threading.Channels;
using log4net;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
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

    private static readonly ILog Logger = LogManager.GetLogger(typeof(SendingTasksManager));
    private readonly IServiceProvider _provider;
    private readonly OutboxesManager _outboxesManager;
    private readonly UserGroupTasksPools _groupPools;
    private readonly SendingQuotaOptions _quotas;
    private readonly ConcurrentDictionary<OutboxKey, WorkerInfo> _workers = [];
    private readonly ConcurrentDictionary<long, long> _userOrganizations = [];
    private readonly Channel<bool> _wakeups = Channel.CreateBounded<bool>(
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
        UserGroupTasksPools groupPools,
        IOptions<SendingQuotaOptions> quotas
    )
    {
        _provider = provider;
        _outboxesManager = outboxesManager;
        _groupPools = groupPools;
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
        _wakeups.Writer.TryWrite(true);
        return Task.CompletedTask;
    }

    private async Task DispatchLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _wakeups.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_wakeups.Reader.TryRead(out _)) { }
                DispatchAvailableWorkers(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Logger.Error("发件调度器异常退出", exception);
        }
    }

    private void DispatchAvailableWorkers(CancellationToken cancellationToken)
    {
        while (_workers.Count < _quotas.SystemHardLimit)
        {
            var activeByOrganization = _workers
                .Values.GroupBy(x => x.OrganizationId)
                .ToDictionary(x => x.Key, x => x.Count());
            var activeByUser = _workers
                .Values.GroupBy(x => x.UserId)
                .ToDictionary(x => x.Key, x => x.Count());
            var candidate = _outboxesManager
                .Values.Where(x => !x.ShouldDispose)
                .Where(CanDispatch)
                .Where(x => !_workers.ContainsKey(new OutboxKey(x.UserId, x.Id)))
                .OrderBy(x =>
                    GetCount(activeByOrganization, GetOrganizationId(x.UserId))
                    >= _quotas.OrganizationFairShare
                )
                .ThenBy(x => GetCount(activeByUser, x.UserId) >= _quotas.UserFairShare)
                .ThenBy(x => GetCount(activeByOrganization, GetOrganizationId(x.UserId)))
                .ThenBy(x => GetCount(activeByUser, x.UserId))
                .ThenBy(x => x.CreateDate)
                .FirstOrDefault();
            if (candidate is null || !candidate.TryMarkTaskRunning())
                return;

            var key = new OutboxKey(candidate.UserId, candidate.Id);
            var startGate = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var task = RunWorkerAsync(key, candidate, startGate.Task, cancellationToken);
            var worker = new WorkerInfo(
                GetOrganizationId(candidate.UserId),
                candidate.UserId,
                task
            );
            if (_workers.TryAdd(key, worker))
            {
                startGate.SetResult(true);
                continue;
            }

            candidate.MarkTaskStopped();
            startGate.SetResult(false);
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
        Logger.Info($"开始执行发件任务: {key} {outbox.Email}");
        Contexts.SendingContext? activeContext = null;
        try
        {
            while (!cancellationToken.IsCancellationRequested && !outbox.ShouldDispose)
            {
                await using var scope = _provider.CreateAsyncScope();
                var sendingContext = scope
                    .ServiceProvider.GetRequiredService<Contexts.SendingContext>()
                    .SetOutbox(outbox);
                activeContext = sendingContext;
                var pipeline = scope.ServiceProvider.GetRequiredService<ISendingPipeline>();
                await pipeline.Handle(sendingContext);
                if (sendingContext.ShouldExitTask())
                    break;
                if (sendingContext.EmailItem is null)
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (activeContext?.EmailItem is { } item)
                activeContext.GroupTask?.ReleaseEmailItem(item);
            Logger.Error($"发件箱 {key} 发件任务异常终止", exception);
        }
        finally
        {
            outbox.MarkTaskStopped();
            _workers.TryRemove(key, out _);
            _wakeups.Writer.TryWrite(true);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _wakeups.Writer.TryComplete();
        await _shutdown.CancelAsync();
        try
        {
            await _dispatcher;
            await Task.WhenAll(_workers.Values.Select(x => x.Task));
        }
        catch (OperationCanceledException) { }
        _shutdown.Dispose();
    }

    private static int GetCount(Dictionary<long, int> counts, long key) =>
        counts.TryGetValue(key, out var count) ? count : 0;

    private long GetOrganizationId(long userId) =>
        _userOrganizations.TryGetValue(userId, out var organizationId) ? organizationId : 0;

    private bool CanDispatch(OutboxEmailAddress outbox) =>
        _groupPools.TryGetValue(outbox.UserId, out var pool) && pool.MatchReadyEmailItem(outbox);
}
