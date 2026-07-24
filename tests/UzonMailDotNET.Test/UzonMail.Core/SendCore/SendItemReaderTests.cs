using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Reading;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SendItemReaderTests
{
    [TestMethod]
    public async Task Reader_UsesKeysetPagesWithoutDuplicates()
    {
        var source = new MemoryPageSource(
            [
                new SendItemDescriptor(1, 10, 0, 0),
                new SendItemDescriptor(2, 10, 0, 0),
                new SendItemDescriptor(3, 10, 8, 0),
                new SendItemDescriptor(4, 10, 8, 0),
                new SendItemDescriptor(5, 10, 9, 0),
            ]
        );
        await using var provider = CreateProvider(source);
        var pool = CreatePool(provider, pageSize: 2, globalCapacity: 4);
        var reader = pool.Open(10, [1, 2, 3, 4, 5], includePending: true);
        var actual = new List<long>();

        while (!reader.IsCompleted)
        {
            await reader.EnsureBufferedAsync();
            while (reader.TryRead(out var descriptor))
                actual.Add(descriptor!.Id);
        }

        CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4, 5 }, actual);
        Assert.IsTrue(source.Requests.All(x => x.IncludePending));
        Assert.AreEqual(0, pool.BufferedDescriptorCount);
    }

    [TestMethod]
    public async Task Reader_GlobalCapacityAppliesBackpressureAndIsReleasedOnClose()
    {
        var source = new MemoryPageSource(
            [
                new SendItemDescriptor(1, 10, 0, 0),
                new SendItemDescriptor(2, 10, 0, 0),
                new SendItemDescriptor(3, 20, 0, 0),
                new SendItemDescriptor(4, 20, 0, 0),
            ]
        );
        await using var provider = CreateProvider(source);
        var pool = CreatePool(provider, pageSize: 2, globalCapacity: 2);
        var first = pool.Open(10);
        var second = pool.Open(20);

        Assert.IsTrue(await first.EnsureBufferedAsync());
        Assert.IsFalse(await second.EnsureBufferedAsync());
        Assert.AreEqual(2, pool.BufferedDescriptorCount);

        Assert.IsTrue(pool.Close(10));
        Assert.AreEqual(0, pool.BufferedDescriptorCount);
        Assert.IsTrue(await second.EnsureBufferedAsync());
        Assert.AreEqual(2, second.BufferedCount);
    }

    private static ServiceProvider CreateProvider(MemoryPageSource source)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISendItemPageSource>(source);
        return services.BuildServiceProvider();
    }

    private static SendItemReaderPool CreatePool(
        ServiceProvider provider,
        int pageSize,
        int globalCapacity
    )
    {
        return new SendItemReaderPool(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(
                new SendItemReaderOptions
                {
                    PageSize = pageSize,
                    MaxBufferedPerGroup = pageSize * 2,
                    MaxBufferedGlobally = globalCapacity,
                    MaxActiveReaders = 4,
                }
            )
        );
    }

    private sealed class MemoryPageSource(IReadOnlyList<SendItemDescriptor> items)
        : ISendItemPageSource
    {
        public List<SendItemPageRequest> Requests { get; } = [];

        public Task<IReadOnlyList<SendItemDescriptor>> ReadPageAsync(
            SendItemPageRequest request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(request);
            var selected = request.SelectedItemIds?.ToHashSet();
            IReadOnlyList<SendItemDescriptor> page = items
                .Where(x => x.SendingGroupId == request.SendingGroupId)
                .Where(x => selected is null || selected.Contains(x.Id))
                .Where(x =>
                    x.OutboxId > request.Cursor.OutboxId
                    || (x.OutboxId == request.Cursor.OutboxId && x.Id > request.Cursor.Id)
                )
                .OrderBy(x => x.OutboxId)
                .ThenBy(x => x.Id)
                .Take(request.Take)
                .ToList();
            return Task.FromResult(page);
        }
    }
}
