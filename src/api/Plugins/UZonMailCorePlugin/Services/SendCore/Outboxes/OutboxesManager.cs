using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Outboxes;

public sealed class OutboxesManager : ISingletonService
{
    private readonly ConcurrentDictionary<OutboxKey, OutboxEmailAddress> _outboxes = [];
    private readonly ConcurrentDictionary<
        long,
        ConcurrentDictionary<OutboxKey, byte>
    > _groupOutboxes = [];
    private readonly object _registrationLock = new();

    public IReadOnlyCollection<OutboxEmailAddress> Values => [.. _outboxes.Values];

    public int Count => _outboxes.Count;

    public void AddOutbox(OutboxEmailAddress outbox)
    {
        lock (_registrationLock)
        {
            var key = GetKey(outbox);
            var registeredOutbox = _outboxes.AddOrUpdate(
                key,
                outbox,
                (_, existing) =>
                {
                    existing.Update(outbox);
                    return existing;
                }
            );
            foreach (var groupId in registeredOutbox.GetSendingGroupIds())
            {
                _groupOutboxes.GetOrAdd(groupId, static _ => []).TryAdd(key, 0);
            }
        }
    }

    public bool RemoveOutbox(OutboxEmailAddress outbox, string message)
    {
        lock (_registrationLock)
        {
            var key = GetKey(outbox);
            if (!_outboxes.TryRemove(key, out var removed))
                return false;
            foreach (var groupId in removed.GetSendingGroupIds())
            {
                if (_groupOutboxes.TryGetValue(groupId, out var groupOutboxes))
                    groupOutboxes.TryRemove(key, out _);
            }
            removed.MarkShouldDispose(message);
            return true;
        }
    }

    public List<OutboxEmailAddress> RemoveOutbox(long sendingGroupId, string message)
    {
        lock (_registrationLock)
        {
            List<OutboxEmailAddress> removedResults = [];
            if (!_groupOutboxes.TryRemove(sendingGroupId, out var linkedOutboxes))
                return removedResults;

            foreach (var key in linkedOutboxes.Keys)
            {
                if (!_outboxes.TryGetValue(key, out var outbox))
                    continue;
                outbox.RemoveSendingGroup(sendingGroupId);
                if (outbox.IsWorking || !_outboxes.TryRemove(key, out var removed))
                    continue;
                removed.MarkShouldDispose(message);
                removedResults.Add(removed);
            }
            return removedResults;
        }
    }

    public bool ExistValidOutbox(long sendingGroupId) =>
        _groupOutboxes.TryGetValue(sendingGroupId, out var linkedOutboxes)
        && linkedOutboxes.Keys.Any(key => ExistValidOutbox(key));

    /// <summary>
    /// 判断组内所有仍有效的发件箱是否都在等待每日额度重置。
    /// </summary>
    public bool AreAllOutboxesQuotaBlocked(long sendingGroupId, DateTimeOffset utcNow)
    {
        if (!_groupOutboxes.TryGetValue(sendingGroupId, out var linkedOutboxes))
            return false;

        var validOutboxes = linkedOutboxes
            .Keys.Select(key => _outboxes.TryGetValue(key, out var outbox) ? outbox : null)
            .Where(outbox => outbox is { ShouldDispose: false })
            .ToList();
        return validOutboxes.Count > 0
            && validOutboxes.All(outbox => outbox!.IsQuotaBlocked(utcNow));
    }

    public bool ExistValidOutbox(OutboxKey key) =>
        _outboxes.TryGetValue(key, out var outbox) && !outbox.ShouldDispose;

    public bool ExistValidOutbox(string email) =>
        _outboxes.Values.Any(x => x.Email == email && !x.ShouldDispose);

    private static OutboxKey GetKey(OutboxEmailAddress outbox) => new(outbox.UserId, outbox.Id);
}
