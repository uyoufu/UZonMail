using MailKit.Net.Proxy;
using MimeKit;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

/// <summary>
/// Owns one physical SMTP connection and serializes all MailKit operations against it.
/// A retiring entry waits for all leases before disconnecting, preventing use-after-dispose races.
/// </summary>
internal sealed class SmtpSessionEntry(
    SmtpClientKey clientKey,
    ISmtpSession session,
    Func<ISmtpSession, Task> releaseSessionAsync
)
{
    private readonly object _lifecycleLock = new();
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly TaskCompletionSource<bool> _leasesDrained =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int _activeLeaseCount;
    private SmtpSessionState _state = SmtpSessionState.Active;
    private DateTimeOffset _lastUsedAtUtc = DateTimeOffset.UtcNow;
    private Task? _retirementTask;

    public Guid InstanceId { get; } = Guid.CreateVersion7();

    public SmtpClientKey ClientKey { get; } = clientKey;

    public IProxyClient? ProxyClient => session.ProxyClient;

    public bool IsIdleExpired(DateTimeOffset utcNow, TimeSpan idleTimeout)
    {
        lock (_lifecycleLock)
        {
            return _state == SmtpSessionState.Active && utcNow - _lastUsedAtUtc >= idleTimeout;
        }
    }

    public SmtpSessionLease? TryAcquireLease(Action<SmtpSessionEntry> invalidateSession)
    {
        lock (_lifecycleLock)
        {
            if (_state != SmtpSessionState.Active)
                return null;

            _activeLeaseCount++;
            return new SmtpSessionLease(this, invalidateSession);
        }
    }

    public async Task<int> GetSentCountAsync(CancellationToken cancellationToken)
    {
        await _operationLock.WaitAsync(cancellationToken);
        try
        {
            ThrowIfRetiring();
            return session.SentCount;
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public async Task<string> SendMessageAsync(
        MimeMessage message,
        CancellationToken cancellationToken
    )
    {
        await _operationLock.WaitAsync(cancellationToken);
        try
        {
            ThrowIfRetiring();
            var receiptId = await session.SendMessageAsync(message, cancellationToken);
            lock (_lifecycleLock)
            {
                _lastUsedAtUtc = DateTimeOffset.UtcNow;
            }
            return receiptId;
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public async Task TryKeepAliveAsync(
        DateTimeOffset utcNow,
        TimeSpan idleThreshold,
        CancellationToken cancellationToken
    )
    {
        if (!TryAcquireMaintenanceLease())
            return;

        try
        {
            if (!HasBeenIdleFor(utcNow, idleThreshold))
                return;

            if (!await _operationLock.WaitAsync(0, cancellationToken))
                return;

            try
            {
                if (!IsActive())
                    return;

                if (!session.IsConnected)
                    throw new IOException("SMTP session is disconnected.");

                await session.NoOpAsync(cancellationToken);
            }
            finally
            {
                _operationLock.Release();
            }
        }
        finally
        {
            await ReleaseLeaseAsync();
        }
    }

    public Task RetireAsync()
    {
        lock (_lifecycleLock)
        {
            if (_retirementTask is not null)
                return _retirementTask;

            _state = SmtpSessionState.Retiring;
            _retirementTask = RetireWhenLeasesDrainAsync();
            return _retirementTask;
        }
    }

    public async ValueTask ReleaseLeaseAsync()
    {
        Task? retirementTask = null;
        lock (_lifecycleLock)
        {
            if (_activeLeaseCount <= 0)
                throw new InvalidOperationException(
                    "SMTP session lease was released more than once."
                );

            _activeLeaseCount--;
            if (_activeLeaseCount == 0)
            {
                _leasesDrained.TrySetResult(true);
                retirementTask = _retirementTask;
            }
        }

        if (retirementTask is not null)
            await retirementTask;
    }

    private bool TryAcquireMaintenanceLease()
    {
        lock (_lifecycleLock)
        {
            if (_state != SmtpSessionState.Active)
                return false;

            _activeLeaseCount++;
            return true;
        }
    }

    private bool HasBeenIdleFor(DateTimeOffset utcNow, TimeSpan idleThreshold)
    {
        lock (_lifecycleLock)
        {
            return _state == SmtpSessionState.Active && utcNow - _lastUsedAtUtc >= idleThreshold;
        }
    }

    private bool IsActive()
    {
        lock (_lifecycleLock)
        {
            return _state == SmtpSessionState.Active;
        }
    }

    private void ThrowIfRetiring()
    {
        if (!IsActive())
            throw new InvalidOperationException("SMTP session has been retired.");
    }

    private async Task RetireWhenLeasesDrainAsync()
    {
        Task leasesDrained;
        lock (_lifecycleLock)
        {
            leasesDrained = _activeLeaseCount == 0 ? Task.CompletedTask : _leasesDrained.Task;
        }

        await leasesDrained;
        await _operationLock.WaitAsync();
        try
        {
            await releaseSessionAsync(session);
        }
        finally
        {
            lock (_lifecycleLock)
            {
                _state = SmtpSessionState.Disposed;
            }
            _operationLock.Release();
        }
    }

    private enum SmtpSessionState
    {
        Active,
        Retiring,
        Disposed,
    }
}
