using System.Collections.Concurrent;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 发件描述符队列。条目只保存一份，FIFO 仅保存可失效的 ID 索引。
/// </summary>
public sealed class SendingItemMetaList
{
    private const int Ready = 0;
    private const int Active = 1;

    private sealed class Entry(SendItemMeta meta)
    {
        public SendItemMeta Meta { get; } = meta;
        public int State = Ready;
    }

    private readonly ConcurrentDictionary<long, Entry> _items = [];
    private readonly ConcurrentQueue<long> _sharedReady = [];
    private readonly ConcurrentDictionary<long, ConcurrentQueue<long>> _specificReady = [];

    public int WaitListCount => _items.Values.Count(x => Volatile.Read(ref x.State) == Ready);

    public int ActiveCount => _items.Values.Count(x => Volatile.Read(ref x.State) == Active);

    public int Count => _items.Count;

    public IEnumerable<long> SendingItemIds => _items.Keys;

    public bool Add(SendItemMeta item)
    {
        var entry = new Entry(item);
        if (!_items.TryAdd(item.SendingItemId, entry))
            return false;

        Enqueue(item);
        return true;
    }

    public bool AddRange(IEnumerable<SendItemMeta> items)
    {
        var added = false;
        foreach (var item in items)
            added |= Add(item);
        return added;
    }

    public SendItemMeta? GetSendingMeta() => TryAcquire(_sharedReady);

    public SendItemMeta? GetSendingMeta(long outboxId)
    {
        return _specificReady.TryGetValue(outboxId, out var queue) ? TryAcquire(queue) : null;
    }

    public bool Complete(SendItemMeta item)
    {
        return _items.TryGetValue(item.SendingItemId, out var entry)
            && ReferenceEquals(entry.Meta, item)
            && Volatile.Read(ref entry.State) == Active
            && _items.TryRemove(new KeyValuePair<long, Entry>(item.SendingItemId, entry));
    }

    public bool Retry(SendItemMeta item)
    {
        if (!Complete(item))
            return false;

        item.IncreaseTriedCount();
        return Add(new SendItemMeta(item.SendingItemId, item.OutboxId, item.TriedCount));
    }

    public bool Release(SendItemMeta item)
    {
        if (!Complete(item))
            return false;
        return Add(new SendItemMeta(item.SendingItemId, item.OutboxId, item.TriedCount));
    }

    public IReadOnlyList<SendItemMeta> GetActiveItems() =>
        [.. _items.Values.Where(x => Volatile.Read(ref x.State) == Active).Select(x => x.Meta)];

    public bool RemovePendingItem(long sendingItemId)
    {
        if (!_items.TryGetValue(sendingItemId, out var entry))
            return true;
        if (Volatile.Read(ref entry.State) != Ready)
            return false;
        if (!_items.TryRemove(new KeyValuePair<long, Entry>(sendingItemId, entry)))
            return false;

        entry.Meta.IsDeleted = true;
        return true;
    }

    public bool MatchSendingMeta(long outboxId, bool onlySpecific)
    {
        return _items.Values.Any(x =>
            !x.Meta.IsDeleted && (onlySpecific ? x.Meta.OutboxId == outboxId : x.Meta.OutboxId == 0)
        );
    }

    public bool MatchReadyMeta(long outboxId, bool onlySpecific)
    {
        return _items.Values.Any(x =>
            Volatile.Read(ref x.State) == Ready
            && !x.Meta.IsDeleted
            && (onlySpecific ? x.Meta.OutboxId == outboxId : x.Meta.OutboxId == 0)
        );
    }

    private void Enqueue(SendItemMeta item)
    {
        if (item.OutboxId <= 0)
        {
            _sharedReady.Enqueue(item.SendingItemId);
            return;
        }

        _specificReady
            .GetOrAdd(item.OutboxId, static _ => new ConcurrentQueue<long>())
            .Enqueue(item.SendingItemId);
    }

    private SendItemMeta? TryAcquire(ConcurrentQueue<long> queue)
    {
        while (queue.TryDequeue(out var itemId))
        {
            if (!_items.TryGetValue(itemId, out var entry))
                continue;
            if (Interlocked.CompareExchange(ref entry.State, Active, Ready) != Ready)
                continue;
            return entry.Meta;
        }
        return null;
    }
}
