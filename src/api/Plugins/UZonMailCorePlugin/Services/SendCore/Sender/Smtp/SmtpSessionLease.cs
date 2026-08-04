using MailKit.Net.Proxy;
using MimeKit;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

/// <summary>
/// Represents an active claim on a cached SMTP session.
/// The lease keeps the underlying connection alive until the caller has completed its send attempt.
/// </summary>
public interface ISmtpSessionLease : IAsyncDisposable
{
    SmtpClientKey ClientKey { get; }

    IProxyClient? ProxyClient { get; }

    Task<int> GetSentCountAsync(CancellationToken cancellationToken = default);

    Task<string> SendMessageAsync(
        MimeMessage message,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stops new callers from using this physical connection after the current leases finish.
    /// This is intentionally non-blocking so a failed sender can release its own lease first.
    /// </summary>
    void Invalidate();
}

/// <inheritdoc />
internal sealed class SmtpSessionLease(
    SmtpSessionEntry entry,
    Action<SmtpSessionEntry> invalidateSession
) : ISmtpSessionLease
{
    private int _isReleased;

    public SmtpClientKey ClientKey => entry.ClientKey;

    public IProxyClient? ProxyClient => entry.ProxyClient;

    public Task<int> GetSentCountAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfReleased();
        return entry.GetSentCountAsync(cancellationToken);
    }

    public Task<string> SendMessageAsync(
        MimeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ThrowIfReleased();
        return entry.SendMessageAsync(message, cancellationToken);
    }

    public void Invalidate()
    {
        ThrowIfReleased();
        invalidateSession(entry);
    }

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isReleased, 1) == 0)
            return entry.ReleaseLeaseAsync();

        return ValueTask.CompletedTask;
    }

    private void ThrowIfReleased()
    {
        if (Volatile.Read(ref _isReleased) != 0)
            throw new ObjectDisposedException(nameof(SmtpSessionLease));
    }
}
