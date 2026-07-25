using Microsoft.Extensions.Options;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;

namespace UzonMailDotNET.Test.UzonMail.DB.Managers.Cache;

/// <summary>
/// 验证依赖版本驱动的缓存刷新与并发语义。
/// </summary>
[TestClass]
public sealed class DBCacheManagerTests
{
    [TestMethod]
    public async Task SharedSourceUpdate_RefreshesEveryResultTypeOnNextRead()
    {
        using var cacheManager = CreateCacheManager();
        using var db = new TestSqlContext();
        var sourceKey = new CacheSourceKey<TestSource, long>(7);
        var argument = new TestCacheArgument(sourceKey);
        await cacheManager.SetSourceAsync(sourceKey, new TestSource(1));

        var firstA = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );
        var firstB = await cacheManager.GetCache<TestCacheB, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );

        Assert.AreSame(
            firstA,
            await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(db, argument)
        );

        await cacheManager.SetSourceAsync(sourceKey, new TestSource(2));
        var secondA = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );
        var secondB = await cacheManager.GetCache<TestCacheB, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );

        Assert.AreEqual(1, firstA.Value);
        Assert.AreEqual(1, firstB.Value);
        Assert.AreEqual(2, secondA.Value);
        Assert.AreEqual(2, secondB.Value);
        Assert.AreNotSame(firstA, secondA);
        Assert.AreNotSame(firstB, secondB);
    }

    [TestMethod]
    public async Task ConcurrentFirstRead_LoadsAndBuildsOnlyOnce()
    {
        using var cacheManager = CreateCacheManager();
        using var db = new TestSqlContext();
        var sourceLoadCount = 0;
        var resultBuildCount = 0;
        var argument = new TestCacheArgument(new CacheSourceKey<TestSource, long>(11))
        {
            SourceLoader = async _ =>
            {
                Interlocked.Increment(ref sourceLoadCount);
                await Task.Yield();
                return new TestSource(3);
            },
            OnBuild = () => Interlocked.Increment(ref resultBuildCount)
        };

        var reads = Enumerable
            .Range(0, 20)
            .Select(_ =>
                cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(db, argument)
            )
            .ToList();
        var results = await Task.WhenAll(reads);

        Assert.AreEqual(1, sourceLoadCount);
        Assert.AreEqual(1, resultBuildCount);
        Assert.IsTrue(results.All(result => ReferenceEquals(results[0], result)));
    }

    [TestMethod]
    public async Task InvalidatedSource_ReloadsOnNextRead()
    {
        using var cacheManager = CreateCacheManager();
        using var db = new TestSqlContext();
        var sourceKey = new CacheSourceKey<TestSource, long>(12);
        var sourceLoadCount = 0;
        var argument = new TestCacheArgument(sourceKey)
        {
            SourceLoader = _ =>
            {
                Interlocked.Increment(ref sourceLoadCount);
                return Task.FromResult(new TestSource(2));
            }
        };
        await cacheManager.SetSourceAsync(sourceKey, new TestSource(1));
        var previous = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );

        await cacheManager.InvalidateSourceAsync(sourceKey);
        var refreshed = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );

        Assert.AreEqual(1, sourceLoadCount);
        Assert.AreEqual(1, previous.Value);
        Assert.AreEqual(2, refreshed.Value);
        Assert.AreNotSame(previous, refreshed);
    }

    [TestMethod]
    public async Task SourceChangesDuringBuild_DiscardsOutdatedResult()
    {
        using var cacheManager = CreateCacheManager();
        using var db = new TestSqlContext();
        var sourceKey = new CacheSourceKey<TestSource, long>(13);
        await cacheManager.SetSourceAsync(sourceKey, new TestSource(1));
        var firstBuildEntered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var releaseFirstBuild = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var buildCount = 0;
        var argument = new TestCacheArgument(sourceKey)
        {
            AfterSourceRead = async _ =>
            {
                if (Interlocked.Increment(ref buildCount) != 1)
                    return;

                firstBuildEntered.SetResult();
                await releaseFirstBuild.Task;
            }
        };

        var pendingResult = cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );
        await firstBuildEntered.Task;
        await cacheManager.SetSourceAsync(sourceKey, new TestSource(2));
        releaseFirstBuild.SetResult();

        var result = await pendingResult;
        Assert.AreEqual(2, result.Value);
        Assert.AreEqual(2, buildCount);
    }

    [TestMethod]
    public async Task FailedRefresh_ThrowsAndKeepsPreviousSnapshotForRetry()
    {
        using var cacheManager = CreateCacheManager();
        using var db = new TestSqlContext();
        var sourceKey = new CacheSourceKey<TestSource, long>(17);
        var shouldFail = false;
        var argument = new TestCacheArgument(sourceKey) { ShouldFail = () => shouldFail };
        await cacheManager.SetSourceAsync(sourceKey, new TestSource(1));
        var previous = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );

        await cacheManager.SetSourceAsync(sourceKey, new TestSource(2));
        shouldFail = true;
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(db, argument)
        );

        shouldFail = false;
        var refreshed = await cacheManager.GetCache<TestCacheA, TestSqlContext, TestCacheArgument>(
            db,
            argument
        );
        Assert.AreEqual(1, previous.Value);
        Assert.AreEqual(2, refreshed.Value);
        Assert.AreNotSame(previous, refreshed);
    }

    private static DBCacheManager CreateCacheManager() =>
        new(
            Options.Create(
                new DBCacheOptions
                {
                    SlidingExpirationMinutes = 30,
                    ExpirationScanFrequencyMinutes = 5
                }
            )
        );

    private sealed record TestSource(int Value);

    private sealed class TestCacheArgument(CacheSourceKey<TestSource, long> sourceKey)
    {
        public CacheSourceKey<TestSource, long> SourceKey { get; } = sourceKey;

        public Func<CancellationToken, Task<TestSource>> SourceLoader { get; init; } =
            _ => Task.FromResult(new TestSource(0));

        public Func<CancellationToken, Task> AfterSourceRead { get; init; } =
            _ => Task.CompletedTask;

        public Func<bool> ShouldFail { get; init; } = () => false;

        public Action OnBuild { get; init; } = () => { };
    }

    private sealed class TestCacheA : BaseDBCache<TestSqlContext, TestCacheArgument>
    {
        public int Value { get; private set; }

        protected override async Task UpdateCore(
            CacheBuildContext buildContext,
            TestSqlContext db,
            CancellationToken cancellationToken
        )
        {
            Args.OnBuild();
            var source = await buildContext.GetSourceAsync(
                Args.SourceKey,
                Args.SourceLoader,
                cancellationToken
            );
            await Args.AfterSourceRead(cancellationToken);
            if (Args.ShouldFail())
                throw new InvalidOperationException("Test refresh failed.");

            Value = source.Value;
        }
    }

    private sealed class TestCacheB : BaseDBCache<TestSqlContext, TestCacheArgument>
    {
        public int Value { get; private set; }

        protected override async Task UpdateCore(
            CacheBuildContext buildContext,
            TestSqlContext db,
            CancellationToken cancellationToken
        )
        {
            var source = await buildContext.GetSourceAsync(
                Args.SourceKey,
                Args.SourceLoader,
                cancellationToken
            );
            Value = source.Value;
        }
    }

    private sealed class TestSqlContext : SqlContextBase;
}
