using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Reading;

public interface ISendItemReaderPool
{
    SendItemReaderSession Open(
        long sendingGroupId,
        IReadOnlyCollection<long>? selectedItemIds = null,
        bool includePending = false
    );

    bool TryGet(long sendingGroupId, out SendItemReaderSession? reader);

    bool Close(long sendingGroupId);

    int ActiveReaderCount { get; }

    int BufferedDescriptorCount { get; }
}

public sealed class SendItemReaderPool(
    IServiceScopeFactory scopeFactory,
    IOptions<SendItemReaderOptions> options
) : ISendItemReaderPool, ISingletonService<ISendItemReaderPool>
{
    private readonly Lock _sync = new();
    private readonly Dictionary<long, SendItemReaderSession> _readers = [];
    private readonly SendItemReaderOptions _options = Validate(options.Value);
    private int _bufferedDescriptorCount;

    public int ActiveReaderCount
    {
        get
        {
            lock (_sync)
                return _readers.Count;
        }
    }

    public int BufferedDescriptorCount => Volatile.Read(ref _bufferedDescriptorCount);

    public SendItemReaderSession Open(
        long sendingGroupId,
        IReadOnlyCollection<long>? selectedItemIds = null,
        bool includePending = false
    )
    {
        lock (_sync)
        {
            if (_readers.TryGetValue(sendingGroupId, out var existing))
                return existing;

            if (_readers.Count >= _options.MaxActiveReaders)
                throw new InvalidOperationException(
                    $"Active send item readers reached the limit {_options.MaxActiveReaders}."
                );

            var reader = new SendItemReaderSession(
                this,
                scopeFactory,
                _options,
                sendingGroupId,
                selectedItemIds,
                includePending
            );
            _readers.Add(sendingGroupId, reader);
            return reader;
        }
    }

    public bool TryGet(long sendingGroupId, out SendItemReaderSession? reader)
    {
        lock (_sync)
            return _readers.TryGetValue(sendingGroupId, out reader);
    }

    public bool Close(long sendingGroupId)
    {
        SendItemReaderSession? reader;
        lock (_sync)
        {
            if (!_readers.Remove(sendingGroupId, out reader))
                return false;
        }

        reader.Close();
        return true;
    }

    internal int Reserve(int requested)
    {
        while (true)
        {
            var current = Volatile.Read(ref _bufferedDescriptorCount);
            var available = _options.MaxBufferedGlobally - current;
            if (available <= 0)
                return 0;

            var reserved = Math.Min(requested, available);
            if (
                Interlocked.CompareExchange(
                    ref _bufferedDescriptorCount,
                    current + reserved,
                    current
                ) == current
            )
                return reserved;
        }
    }

    internal void Release(int count)
    {
        if (count <= 0)
            return;
        var remaining = Interlocked.Add(ref _bufferedDescriptorCount, -count);
        if (remaining < 0)
            throw new InvalidOperationException("Send item reader capacity was released twice.");
    }

    private static SendItemReaderOptions Validate(SendItemReaderOptions options)
    {
        options.Validate();
        return options;
    }
}

public sealed class SendItemReaderSession
{
    private readonly SendItemReaderPool _pool;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SendItemReaderOptions _options;
    private readonly IReadOnlyCollection<long>? _selectedItemIds;
    private readonly bool _includePending;
    private readonly SemaphoreSlim _readLock = new(1, 1);
    private readonly Lock _sync = new();
    private readonly Queue<SendItemDescriptor> _buffer = [];
    private SendItemCursor _cursor = SendItemCursor.Start;
    private bool _completed;
    private bool _closed;

    internal SendItemReaderSession(
        SendItemReaderPool pool,
        IServiceScopeFactory scopeFactory,
        SendItemReaderOptions options,
        long sendingGroupId,
        IReadOnlyCollection<long>? selectedItemIds,
        bool includePending
    )
    {
        _pool = pool;
        _scopeFactory = scopeFactory;
        _options = options;
        SendingGroupId = sendingGroupId;
        _selectedItemIds = selectedItemIds is null ? null : [.. selectedItemIds.Distinct()];
        _includePending = includePending;
    }

    public long SendingGroupId { get; }

    public bool IsCompleted
    {
        get
        {
            lock (_sync)
                return _completed && _buffer.Count == 0;
        }
    }

    public int BufferedCount
    {
        get
        {
            lock (_sync)
                return _buffer.Count;
        }
    }

    public async Task<bool> EnsureBufferedAsync(CancellationToken cancellationToken = default)
    {
        await _readLock.WaitAsync(cancellationToken);
        try
        {
            int currentCount;
            SendItemCursor cursor;
            lock (_sync)
            {
                ObjectDisposedException.ThrowIf(_closed, this);
                if (_completed)
                    return _buffer.Count > 0;
                currentCount = _buffer.Count;
                cursor = _cursor;
            }

            var localCapacity = _options.MaxBufferedPerGroup - currentCount;
            if (localCapacity <= 0)
                return true;

            var requested = Math.Min(_options.PageSize, localCapacity);
            var reserved = _pool.Reserve(requested);
            if (reserved == 0)
                return currentCount > 0;

            IReadOnlyList<SendItemDescriptor> page;
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var source = scope.ServiceProvider.GetRequiredService<ISendItemPageSource>();
                page = await source.ReadPageAsync(
                    new SendItemPageRequest(
                        SendingGroupId,
                        cursor,
                        reserved,
                        _selectedItemIds,
                        _includePending
                    ),
                    cancellationToken
                );
            }
            catch
            {
                _pool.Release(reserved);
                throw;
            }

            _pool.Release(reserved - page.Count);
            lock (_sync)
            {
                if (_closed)
                {
                    _pool.Release(page.Count);
                    return false;
                }
                foreach (var item in page)
                    _buffer.Enqueue(item);

                if (page.Count > 0)
                {
                    var last = page[^1];
                    _cursor = new SendItemCursor(last.SenderAccountId, last.Id);
                }

                if (page.Count < reserved)
                    _completed = true;

                return _buffer.Count > 0;
            }
        }
        finally
        {
            _readLock.Release();
        }
    }

    public bool TryRead(out SendItemDescriptor? descriptor)
    {
        lock (_sync)
        {
            if (!_buffer.TryDequeue(out descriptor))
                return false;
        }

        _pool.Release(1);
        return true;
    }

    internal void Close()
    {
        int count;
        lock (_sync)
        {
            if (_closed)
                return;
            _closed = true;
            count = _buffer.Count;
            _buffer.Clear();
        }

        _pool.Release(count);
    }
}
