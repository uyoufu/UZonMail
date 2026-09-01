using System.Collections.Concurrent;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Credentials;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.EmailReceiving;

/// <summary>
/// 为可用收件账号维护 Inbox IDLE，并定期唤醒以同步 Sent 和补偿漏失事件。
/// </summary>
public sealed class ImapIdleCoordinator(
    IServiceScopeFactory scopeFactory,
    ILogger<ImapIdleCoordinator> logger
) : BackgroundService
{
    private static readonly TimeSpan AccountDiscoveryInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan IdleRefreshInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaximumReconnectDelay = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<long, AccountSession> _sessions = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "刷新 IMAP IDLE 账号列表失败");
            }

            await Task.Delay(AccountDiscoveryInterval, stoppingToken);
        }

        foreach (var session in _sessions.Values)
            session.Cancellation.Cancel();
        await Task.WhenAll(_sessions.Values.Select(x => x.Task));
    }

    private async Task ReconcileSessionsAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
        var activeAccounts = await db
            .ReceivingAccounts.AsNoTracking()
            .Where(x =>
                x.Protocol == ReceivingProtocol.Imap
                && x.Status != ReceivingAccountStatus.Paused
                && x.Status != ReceivingAccountStatus.AuthenticationFailed
                && x.Status != ReceivingAccountStatus.ConfigurationRequired
            )
            .Select(x => new AccountIdentity(x.Id, x.EmailAccount.UserId))
            .ToListAsync(stoppingToken);
        var activeAccountIds = activeAccounts.Select(x => x.ReceivingAccountId).ToHashSet();

        foreach (var inactiveSession in _sessions.Where(x => !activeAccountIds.Contains(x.Key)))
        {
            if (_sessions.TryRemove(inactiveSession.Key, out var removedSession))
                removedSession.Cancellation.Cancel();
        }

        foreach (var account in activeAccounts)
        {
            _sessions.GetOrAdd(
                account.ReceivingAccountId,
                _ => StartSession(account, stoppingToken)
            );
        }
    }

    private AccountSession StartSession(AccountIdentity account, CancellationToken stoppingToken)
    {
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var session = new AccountSession(cancellation);
        session.Task = RunAccountSessionAsync(account, cancellation.Token);
        _ = session.Task.ContinueWith(
            _ =>
            {
                if (
                    _sessions.TryGetValue(account.ReceivingAccountId, out var currentSession)
                    && ReferenceEquals(currentSession, session)
                    && _sessions.TryRemove(account.ReceivingAccountId, out var completedSession)
                )
                    completedSession.Cancellation.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
        return session;
    }

    private async Task RunAccountSessionAsync(
        AccountIdentity account,
        CancellationToken stoppingToken
    )
    {
        var reconnectDelay = TimeSpan.FromSeconds(5);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SynchronizeAccountAsync(account, stoppingToken);
                await RunIdleConnectionAsync(account, stoppingToken);
                reconnectDelay = TimeSpan.FromSeconds(5);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "IMAP IDLE 会话异常，账号 {ReceivingAccountId} 将在 {Delay} 后重连",
                    account.ReceivingAccountId,
                    reconnectDelay
                );
                await Task.Delay(reconnectDelay, stoppingToken);
                reconnectDelay = TimeSpan.FromSeconds(
                    Math.Min(MaximumReconnectDelay.TotalSeconds, reconnectDelay.TotalSeconds * 2)
                );
            }
        }
    }

    private async Task RunIdleConnectionAsync(
        AccountIdentity account,
        CancellationToken stoppingToken
    )
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
        var credentialProtector = scope.ServiceProvider.GetRequiredService<ICredentialProtector>();
        var credential =
            await db
                .ReceivingAccountImapCredentials.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ReceivingAccountId == account.ReceivingAccountId,
                    stoppingToken
                ) ?? throw new InvalidOperationException("IMAP 凭据未配置");
        if (
            string.IsNullOrWhiteSpace(credential.EncryptedPassword)
            || string.IsNullOrWhiteSpace(credential.EncryptionKeyVersion)
        )
            throw new InvalidOperationException("IMAP 凭据不完整");

        using var client = new ImapClient();
        await client.ConnectAsync(
            credential.Host,
            credential.Port,
            credential.ConnectionSecurity.ToMailKitSecureSocketOptions(),
            stoppingToken
        );
        var password = credentialProtector.Unprotect(
            credential.EncryptedPassword,
            credential.EncryptionKeyVersion
        );
        await client.AuthenticateAsync(credential.LoginName, password, stoppingToken);
        await ImapClientIdentification.IdentifyAsync(client, stoppingToken);
        await client.Inbox.OpenAsync(FolderAccess.ReadOnly, stoppingToken);

        if (!client.Capabilities.HasFlag(ImapCapabilities.Idle))
        {
            await Task.Delay(IdleRefreshInterval, stoppingToken);
            return;
        }

        using var idleDone = new CancellationTokenSource(IdleRefreshInterval);
        void OnMailboxChanged(object? _, EventArgs __) => idleDone.Cancel();
        client.Inbox.CountChanged += OnMailboxChanged;
        client.Inbox.MessageExpunged += OnMailboxChanged;
        client.Inbox.MessageFlagsChanged += OnMailboxChanged;
        try
        {
            await client.IdleAsync(idleDone.Token, stoppingToken);
        }
        finally
        {
            client.Inbox.CountChanged -= OnMailboxChanged;
            client.Inbox.MessageExpunged -= OnMailboxChanged;
            client.Inbox.MessageFlagsChanged -= OnMailboxChanged;
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
    }

    private async Task SynchronizeAccountAsync(
        AccountIdentity account,
        CancellationToken cancellationToken
    )
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var synchronizationService =
            scope.ServiceProvider.GetRequiredService<IReceivingSynchronizationService>();
        await synchronizationService.SynchronizeAsync(
            account.UserId,
            account.ReceivingAccountId,
            ImapSyncTrigger.Scheduled,
            cancellationToken
        );
    }

    private sealed record AccountIdentity(long ReceivingAccountId, long UserId);

    private sealed class AccountSession(CancellationTokenSource cancellation)
    {
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public Task Task { get; set; } = Task.CompletedTask;
    }
}
