using System.Collections.Concurrent;
using log4net;
using MailKit.Net.Proxy;
using MailKit.Security;
using MimeKit;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Results;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

/// <summary>
/// Represents a cached SMTP connection that can be safely managed by the SMTP session manager.
/// </summary>
public interface ISmtpSession : IDisposable
{
    bool IsConnected { get; }

    IProxyClient? ProxyClient { get; set; }

    int SentCount { get; }

    void SetParams(SmtpClientKey clientKey, int cooldownMilliseconds);

    Task ConnectAsync(
        string host,
        int port,
        SecureSocketOptions options,
        CancellationToken cancellationToken = default
    );

    Task AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default
    );

    Task NoOpAsync(CancellationToken cancellationToken = default);

    Task<string> SendMessageAsync(
        MimeMessage message,
        CancellationToken cancellationToken = default
    );

    Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);
}

/// <summary>
/// Creates SMTP sessions without binding a cached session to an individual request scope.
/// </summary>
public interface ISmtpSessionFactory
{
    ISmtpSession Create();
}

/// <summary>
/// Manages SMTP sessions isolated by senderAccount, protocol profile, and network route.
/// </summary>
public interface ISmtpClientsManager
{
    IReadOnlyCollection<SmtpClientKey> SmtpClientKeys { get; }

    Task<Result<ISmtpSessionLease>> AcquireSmtpSessionAsync(
        SendingContext context,
        NetworkRoute route,
        CancellationToken cancellationToken = default
    );

    Task DisposeSmtpClientAsync(SmtpClientKey key);

    Task DisposeSmtpClientsAsync(SenderAccountKey senderAccount);
}

/// <inheritdoc />
public sealed class SmtpSessionFactory(DebugConfig debugConfig)
    : ISmtpSessionFactory,
        ISingletonService<ISmtpSessionFactory>
{
    public ISmtpSession Create() => new ThrottlingSmtpClient(debugConfig);
}

/// <summary>
/// Caches SMTP sessions and coordinates their use, probing, retirement, and disposal.
/// </summary>
public sealed class SmtpClientsManager
    : ISmtpClientsManager,
        ISingletonService<ISmtpClientsManager>,
        IAsyncDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SmtpClientsManager));
    private static readonly TimeSpan MaintenanceInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan IdleKeepAliveThreshold = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan IdleSessionTimeout = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<SmtpClientKey, SmtpSessionEntry> _clients = [];
    private readonly ConcurrentDictionary<
        SmtpClientKey,
        Lazy<Task<SmtpSessionEntry>>
    > _creatingClients = [];
    private readonly AppSettingsManager _settingsService;
    private readonly SmtpConnector _connector;
    private readonly ISmtpSessionFactory _sessionFactory;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _maintenanceTask;

    public SmtpClientsManager(
        AppSettingsManager settingsService,
        SmtpConnector connector,
        ISmtpSessionFactory sessionFactory
    )
    {
        _settingsService = settingsService;
        _connector = connector;
        _sessionFactory = sessionFactory;
        _maintenanceTask = MaintainConnectionsAsync(_shutdown.Token);
    }

    public IReadOnlyCollection<SmtpClientKey> SmtpClientKeys => _clients.Keys.ToArray();

    public async Task<Result<ISmtpSessionLease>> AcquireSmtpSessionAsync(
        SendingContext context,
        NetworkRoute route,
        CancellationToken cancellationToken = default
    )
    {
        if (_shutdown.IsCancellationRequested)
            return Result<ISmtpSessionLease>.Fail("SMTP 会话管理器正在停止");

        var senderAccount = context.CurrentAttempt!.PreparedItem.SenderAccount;
        var key = new SmtpClientKey(
            new SenderAccountKey(senderAccount.UserId, senderAccount.Id),
            GetProfileFingerprint(senderAccount),
            route.Identity,
            senderAccount.Email
        );
        var profile = new SmtpConnectionProfile(
            senderAccount.SmtpHost,
            senderAccount.SmtpPort,
            senderAccount.ConnectionSecurity.ToMailKitSecureSocketOptions(),
            senderAccount.SmtpAuthUserName ?? senderAccount.Email,
            senderAccount.PlainPassword ?? string.Empty,
            context.Provider.GetRequiredService<DebugConfig>().IsDemo
        );

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_clients.TryGetValue(key, out var cached))
                {
                    var cachedLease = await TryAcquireReusableLeaseAsync(
                        cached,
                        context,
                        cancellationToken
                    );
                    if (cachedLease is not null)
                        return new Result<ISmtpSessionLease> { Data = cachedLease };

                    continue;
                }

                var created = await GetOrCreateSessionEntryAsync(
                    key,
                    profile,
                    route.ProxyClient,
                    cancellationToken
                );
                if (_shutdown.IsCancellationRequested)
                {
                    await RetireEntryAsync(
                        key,
                        created,
                        SmtpSessionRetirementReason.ManagerShutdown
                    );
                    return Result<ISmtpSessionLease>.Fail("SMTP 会话管理器正在停止");
                }

                if (
                    !_clients.TryGetValue(key, out var current)
                    || !ReferenceEquals(current, created)
                )
                    continue;

                var createdLease = created.TryAcquireLease(InvalidateEntry);
                if (createdLease is not null)
                    return new Result<ISmtpSessionLease> { Data = createdLease };
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            Logger.Warn($"创建 SMTP 会话 {key} 失败", exception);
            return Result<ISmtpSessionLease>.Fail(exception.Message);
        }
    }

    public async Task DisposeSmtpClientAsync(SmtpClientKey key)
    {
        if (_clients.TryGetValue(key, out var client))
            await RetireEntryAsync(key, client, SmtpSessionRetirementReason.ExplicitDisposal);
    }

    public async Task DisposeSmtpClientsAsync(SenderAccountKey senderAccount)
    {
        var entries = _clients.Where(entry => entry.Key.SenderAccount == senderAccount).ToArray();
        foreach (var entry in entries)
            await RetireEntryAsync(
                entry.Key,
                entry.Value,
                SmtpSessionRetirementReason.SenderAccountDisposal
            );
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdown.CancelAsync();
        try
        {
            await _maintenanceTask;
        }
        catch (OperationCanceledException) { }

        var entries = _clients.ToArray();
        foreach (var entry in entries)
            TryRemoveCurrentEntry(entry.Key, entry.Value);
        foreach (var entry in entries)
            LogRetirement(entry.Value, SmtpSessionRetirementReason.ManagerShutdown);
        await Task.WhenAll(entries.Select(entry => entry.Value.RetireAsync()));
        _shutdown.Dispose();
    }

    private async Task<ISmtpSessionLease?> TryAcquireReusableLeaseAsync(
        SmtpSessionEntry entry,
        SendingContext context,
        CancellationToken cancellationToken
    )
    {
        var lease = entry.TryAcquireLease(InvalidateEntry);
        if (lease is null)
            return null;

        try
        {
            if (!await IsReusableForContextAsync(lease, context, cancellationToken))
            {
                lease.Invalidate();
                await lease.DisposeAsync();
                return null;
            }

            return lease;
        }
        catch
        {
            lease.Invalidate();
            await lease.DisposeAsync();
            throw;
        }
    }

    private async Task<bool> IsReusableForContextAsync(
        ISmtpSessionLease lease,
        SendingContext context,
        CancellationToken cancellationToken
    )
    {
        if (!lease.ClientKey.HasProxy)
            return true;
        if (lease.ProxyClient is not ProxyClientAdapter proxy || !proxy.IsEnable)
            return false;

        var setting = await _settingsService.GetSetting<SendingSetting>(
            context.SqlContext,
            context.CurrentAttempt!.PreparedItem.UserId
        );
        var sentCount = await lease.GetSentCountAsync(cancellationToken);
        return setting.ChangeIpAfterEmailCount <= 0
            || sentCount == 0
            || sentCount % setting.ChangeIpAfterEmailCount != 0;
    }

    private async Task<SmtpSessionEntry> GetOrCreateSessionEntryAsync(
        SmtpClientKey key,
        SmtpConnectionProfile profile,
        IProxyClient? proxyClient,
        CancellationToken cancellationToken
    )
    {
        var newCreation = new Lazy<Task<SmtpSessionEntry>>(
            () => CreateSessionEntryAsync(key, profile, proxyClient),
            LazyThreadSafetyMode.ExecutionAndPublication
        );
        var pendingCreation = _creatingClients.GetOrAdd(key, newCreation);
        var creationTask = pendingCreation.Value;
        _ = creationTask.ContinueWith(
            _ => TryRemovePendingCreation(key, pendingCreation),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
        return await creationTask.WaitAsync(cancellationToken);
    }

    private async Task<SmtpSessionEntry> CreateSessionEntryAsync(
        SmtpClientKey key,
        SmtpConnectionProfile profile,
        IProxyClient? proxyClient
    )
    {
        var client = _sessionFactory.Create();
        SmtpSessionEntry? createdEntry = null;
        client.SetParams(key, 0);
        client.ProxyClient = proxyClient;
        try
        {
            await _connector.ConnectAndAuthenticateAsync(client, profile, _shutdown.Token);
            _shutdown.Token.ThrowIfCancellationRequested();

            createdEntry = new SmtpSessionEntry(key, client, DisconnectAndDisposeAsync);
            if (_clients.TryAdd(key, createdEntry))
                return createdEntry;

            await createdEntry.RetireAsync();
            if (_clients.TryGetValue(key, out var current))
                return current;

            throw new InvalidOperationException("SMTP 会话创建结果未能发布到缓存。");
        }
        catch
        {
            if (createdEntry is null)
                await DisconnectAndDisposeAsync(client);
            throw;
        }
    }

    private async Task MaintainConnectionsAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(MaintenanceInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var utcNow = DateTimeOffset.UtcNow;
                foreach (var entry in _clients.ToArray())
                {
                    if (entry.Value.IsIdleExpired(utcNow, IdleSessionTimeout))
                    {
                        await RetireEntryAsync(
                            entry.Key,
                            entry.Value,
                            SmtpSessionRetirementReason.IdleTimeout
                        );
                        continue;
                    }

                    try
                    {
                        await entry.Value.TryKeepAliveAsync(
                            utcNow,
                            IdleKeepAliveThreshold,
                            cancellationToken
                        );
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        Logger.Warn($"SMTP 会话 {entry.Key} 保活失败", exception);
                        await RetireEntryAsync(
                            entry.Key,
                            entry.Value,
                            SmtpSessionRetirementReason.KeepAliveFailure
                        );
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private void InvalidateEntry(SmtpSessionEntry entry)
    {
        if (TryRemoveCurrentEntry(entry.ClientKey, entry))
            LogRetirement(entry, SmtpSessionRetirementReason.SessionInvalidated);
        _ = entry.RetireAsync();
    }

    private async Task RetireEntryAsync(
        SmtpClientKey key,
        SmtpSessionEntry entry,
        SmtpSessionRetirementReason reason
    )
    {
        if (TryRemoveCurrentEntry(key, entry))
            LogRetirement(entry, reason);
        await entry.RetireAsync();
    }

    private static void LogRetirement(SmtpSessionEntry entry, SmtpSessionRetirementReason reason) =>
        Logger.Debug($"SMTP 会话 {entry.ClientKey} 实例 {entry.InstanceId} 开始退役，原因：{reason}");

    private bool TryRemoveCurrentEntry(SmtpClientKey key, SmtpSessionEntry expectedEntry) =>
        ((ICollection<KeyValuePair<SmtpClientKey, SmtpSessionEntry>>)_clients).Remove(
            new KeyValuePair<SmtpClientKey, SmtpSessionEntry>(key, expectedEntry)
        );

    private void TryRemovePendingCreation(
        SmtpClientKey key,
        Lazy<Task<SmtpSessionEntry>> expectedCreation
    ) =>
        (
            (ICollection<KeyValuePair<SmtpClientKey, Lazy<Task<SmtpSessionEntry>>>>)_creatingClients
        ).Remove(
            new KeyValuePair<SmtpClientKey, Lazy<Task<SmtpSessionEntry>>>(key, expectedCreation)
        );

    private static async Task DisconnectAndDisposeAsync(ISmtpSession client)
    {
        try
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
        catch (Exception exception)
        {
            Logger.Debug("释放 SMTP 会话时断开连接失败", exception);
        }
        finally
        {
            try
            {
                client.Dispose();
            }
            catch (Exception exception)
            {
                Logger.Debug("释放 SMTP 会话资源失败", exception);
            }
        }
    }

    private static string GetProfileFingerprint(SenderEmailAddress senderAccount) =>
        $"{senderAccount.SmtpHost}\n{senderAccount.SmtpPort}\n{senderAccount.SmtpAuthUserName}\n{senderAccount.PlainPassword}\n{senderAccount.ConnectionSecurity}".MD5();

    private enum SmtpSessionRetirementReason
    {
        ExplicitDisposal,
        IdleTimeout,
        KeepAliveFailure,
        ManagerShutdown,
        SenderAccountDisposal,
        SessionInvalidated,
    }
}
