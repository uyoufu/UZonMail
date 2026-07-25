using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMail.CorePlugin.Services.SendCore.WaitList;

/// <summary>
/// 发件描述符队列。完整载荷只在取得描述符和租约后按需加载。
/// </summary>
public sealed class SendItemQueue
{
    private const int Ready = 0;
    private const int Active = 1;

    private sealed class Entry(SendItemDescriptor descriptor)
    {
        public SendItemDescriptor Descriptor { get; } = descriptor;
        public int State = Ready;
    }

    private readonly ConcurrentDictionary<long, Entry> _items = [];
    private readonly ConcurrentQueue<long> _sharedReady = [];
    private readonly ConcurrentDictionary<long, ConcurrentQueue<long>> _specificReady = [];

    /// <summary>当前可获取的描述符数量。</summary>
    public int ReadyCount => _items.Values.Count(x => Volatile.Read(ref x.State) == Ready);

    /// <summary>当前已取得、尚未提交的描述符数量。</summary>
    public int ActiveCount => _items.Values.Count(x => Volatile.Read(ref x.State) == Active);

    /// <summary>队列中的描述符总数。</summary>
    public int Count => _items.Count;

    /// <summary>队列中的全部发件项 ID 快照源。</summary>
    public IEnumerable<long> SendingItemIds => _items.Keys;

    /// <summary>添加一个待发描述符。</summary>
    public bool Add(SendItemDescriptor descriptor)
    {
        var entry = new Entry(descriptor);
        if (!_items.TryAdd(descriptor.Id, entry))
            return false;

        Enqueue(descriptor);
        return true;
    }

    /// <summary>批量添加待发描述符。</summary>
    public bool AddRange(IEnumerable<SendItemDescriptor> descriptors)
    {
        var added = false;
        foreach (var descriptor in descriptors)
            added |= Add(descriptor);
        return added;
    }

    /// <summary>获取一个共享发件箱可发送的描述符。</summary>
    public SendItemDescriptor? AcquireShared() => TryAcquire(_sharedReady);

    /// <summary>获取一个指定发件箱可发送的描述符。</summary>
    public SendItemDescriptor? AcquireSpecific(long outboxId) =>
        _specificReady.TryGetValue(outboxId, out var queue) ? TryAcquire(queue) : null;

    /// <summary>完成并移除活动描述符。</summary>
    public bool Complete(SendItemDescriptor descriptor)
    {
        return _items.TryGetValue(descriptor.Id, out var entry)
            && ReferenceEquals(entry.Descriptor, descriptor)
            && Volatile.Read(ref entry.State) == Active
            && _items.TryRemove(new KeyValuePair<long, Entry>(descriptor.Id, entry));
    }

    /// <summary>将活动描述符原样释放回待发队列。</summary>
    public bool Release(SendItemDescriptor descriptor)
    {
        if (!Complete(descriptor))
            return false;
        return Add(descriptor);
    }

    /// <summary>获取当前所有活动描述符快照。</summary>
    public IReadOnlyList<SendItemDescriptor> GetActiveItems() =>
        [
            .. _items
                .Values.Where(x => Volatile.Read(ref x.State) == Active)
                .Select(x => x.Descriptor),
        ];

    /// <summary>移除尚未被获取的指定描述符。</summary>
    public bool RemovePendingItem(long sendingItemId)
    {
        if (!_items.TryGetValue(sendingItemId, out var entry))
            return true;
        if (Volatile.Read(ref entry.State) != Ready)
            return false;
        return _items.TryRemove(new KeyValuePair<long, Entry>(sendingItemId, entry));
    }

    /// <summary>判断队列是否包含匹配发件箱类型的描述符。</summary>
    public bool Contains(long outboxId, bool onlySpecific)
    {
        return _items.Values.Any(x =>
            onlySpecific ? x.Descriptor.OutboxId == outboxId : x.Descriptor.OutboxId <= 0
        );
    }

    /// <summary>判断待发队列是否包含匹配发件箱类型的描述符。</summary>
    public bool ContainsReady(long outboxId, bool onlySpecific)
    {
        return _items.Values.Any(x =>
            Volatile.Read(ref x.State) == Ready
            && (onlySpecific ? x.Descriptor.OutboxId == outboxId : x.Descriptor.OutboxId <= 0)
        );
    }

    private void Enqueue(SendItemDescriptor descriptor)
    {
        if (descriptor.OutboxId <= 0)
        {
            _sharedReady.Enqueue(descriptor.Id);
            return;
        }

        _specificReady
            .GetOrAdd(descriptor.OutboxId, static _ => new ConcurrentQueue<long>())
            .Enqueue(descriptor.Id);
    }

    private SendItemDescriptor? TryAcquire(ConcurrentQueue<long> queue)
    {
        while (queue.TryDequeue(out var itemId))
        {
            if (!_items.TryGetValue(itemId, out var entry))
                continue;
            if (Interlocked.CompareExchange(ref entry.State, Active, Ready) != Ready)
                continue;
            return entry.Descriptor;
        }
        return null;
    }
}
