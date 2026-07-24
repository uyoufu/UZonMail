using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Outboxes;

public sealed class OutboxesManager : ISingletonService
{
    private readonly ConcurrentDictionary<OutboxKey, OutboxEmailAddress> _outboxes = [];

    public IReadOnlyCollection<OutboxEmailAddress> Values => [.. _outboxes.Values];

    public int Count => _outboxes.Count;

    public void AddOutbox(OutboxEmailAddress outbox)
    {
        var key = GetKey(outbox);
        _outboxes.AddOrUpdate(
            key,
            outbox,
            (_, existing) =>
            {
                existing.Update(outbox);
                return existing;
            }
        );
    }

    public bool RemoveOutbox(OutboxEmailAddress outbox, string message)
    {
        if (!_outboxes.TryRemove(GetKey(outbox), out var removed))
            return false;
        removed.MarkShouldDispose(message);
        return true;
    }

    public List<OutboxEmailAddress> RemoveOutbox(long sendingGroupId, string message)
    {
        List<OutboxEmailAddress> removedResults = [];
        foreach (var pair in _outboxes.ToArray())
        {
            var outbox = pair.Value;
            outbox.RemoveSendingGroup(sendingGroupId);
            if (outbox.IsWorking)
                continue;
            if (!RemoveOutbox(outbox, message))
                continue;
            removedResults.Add(outbox);
        }
        return removedResults;
    }

    public bool ExistValidOutbox(long sendingGroupId) =>
        _outboxes.Values.Any(x => !x.ShouldDispose && x.ContainsSendingGroup(sendingGroupId));

    public bool ExistValidOutbox(OutboxKey key) =>
        _outboxes.TryGetValue(key, out var outbox) && !outbox.ShouldDispose;

    public bool ExistValidOutbox(string email) =>
        _outboxes.Values.Any(x => x.Email == email && !x.ShouldDispose);

    private static OutboxKey GetKey(OutboxEmailAddress outbox) => new(outbox.UserId, outbox.Id);
}
