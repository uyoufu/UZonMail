using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

public sealed class InMemorySendLeaseStore : ISendLeaseStore, ISingletonService<ISendLeaseStore>
{
    private readonly Lock _sync = new();
    private readonly Dictionary<Guid, SendLease> _leases = [];
    private readonly Dictionary<long, Guid> _activeItems = [];

    public bool TryAcquire(
        SendItemDescriptor item,
        SenderAccountKey senderAccount,
        DateTimeOffset now,
        TimeSpan duration,
        out SendLease lease
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);
        lock (_sync)
        {
            if (_activeItems.TryGetValue(item.Id, out var activeLeaseId))
            {
                var activeLease = _leases[activeLeaseId];
                if (activeLease.ExpiresAt > now)
                {
                    lease = null!;
                    return false;
                }

                _leases[activeLeaseId] = activeLease with { State = SendLeaseState.Expired };
                _activeItems.Remove(item.Id);
            }

            lease = new SendLease(Guid.CreateVersion7(), item, senderAccount, now, now + duration);
            _leases.Add(lease.LeaseId, lease);
            _activeItems.Add(item.Id, lease.LeaseId);
            return true;
        }
    }

    public bool TryComplete(Guid leaseId, DateTimeOffset now, out SendLease? lease) =>
        TryFinish(leaseId, now, SendLeaseState.Completed, out lease);

    public bool TryRevoke(Guid leaseId, DateTimeOffset now, out SendLease? lease) =>
        TryFinish(leaseId, now, SendLeaseState.Revoked, out lease);

    public IReadOnlyList<SendLease> ReclaimExpired(DateTimeOffset now)
    {
        lock (_sync)
        {
            var expired = _leases
                .Values.Where(x => x.State == SendLeaseState.Active && x.ExpiresAt <= now)
                .ToList();
            foreach (var current in expired)
            {
                var updated = current with { State = SendLeaseState.Expired };
                _leases[current.LeaseId] = updated;
                _activeItems.Remove(current.Item.Id);
            }

            return expired.ConvertAll(x => x with { State = SendLeaseState.Expired });
        }
    }

    private bool TryFinish(
        Guid leaseId,
        DateTimeOffset now,
        SendLeaseState targetState,
        out SendLease? lease
    )
    {
        lock (_sync)
        {
            if (!_leases.TryGetValue(leaseId, out var current))
            {
                lease = null;
                return false;
            }

            if (current.State != SendLeaseState.Active || current.ExpiresAt <= now)
            {
                if (current.State == SendLeaseState.Active)
                {
                    current = current with { State = SendLeaseState.Expired };
                    _leases[leaseId] = current;
                    _activeItems.Remove(current.Item.Id);
                }

                lease = current;
                return false;
            }

            lease = current with { State = targetState };
            _leases[leaseId] = lease;
            _activeItems.Remove(current.Item.Id);
            return true;
        }
    }
}
