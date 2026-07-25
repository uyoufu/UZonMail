using System.Diagnostics;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.SendCore.Utils;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Utils;

/// <summary>
/// 验证发送核心中的轻量锁、冷却器和 IP 域名限流器。
/// </summary>
[TestClass]
public sealed class ConcurrencyUtilityTests
{
    [TestMethod]
    public void Locker_AllowsOnlyOneOwnerUntilUnlocked()
    {
        var locker = new Locker();

        Assert.IsTrue(locker.Lock());
        Assert.IsTrue(locker.IsLocked);
        Assert.IsFalse(locker.Lock());
        locker.Unlock();
        Assert.IsFalse(locker.IsLocked);
        Assert.IsTrue(locker.Lock());
    }

    [TestMethod]
    public void Cooler_ZeroDurationCompletesImmediatelyAndCanRestart()
    {
        var cooler = new Cooler();
        var callbackCount = 0;

        cooler.StartCooling(0, () => callbackCount++);
        cooler.StartCooling(-1, () => callbackCount++);

        Assert.IsFalse(cooler.IsCooling);
        Assert.AreEqual(2, callbackCount);
    }

    [TestMethod]
    public async Task Cooler_PositiveDurationIgnoresConcurrentStartAndRunsCallback()
    {
        var cooler = new Cooler();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var callbackCount = 0;

        cooler.StartCooling(
            20,
            () =>
            {
                callbackCount++;
                completion.SetResult();
            }
        );
        cooler.StartCooling(1, () => callbackCount += 100);
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(cooler.IsCooling);
        Assert.AreEqual(1, callbackCount);
    }

    [TestMethod]
    public async Task RateLimiter_TracksDomainAndIpWithoutLongWaits()
    {
        using var limiter = new IPRateLimiter();
        Assert.IsFalse(limiter.IsLimited("sender@test.com", "127.0.0.1", 0));

        const int tenMillisecondLimit = 360_000;
        await limiter.WaitForReleaseAsync("sender@test.com", "127.0.0.1", tenMillisecondLimit);

        Assert.IsTrue(limiter.IsLimited("other@test.com", "127.0.0.1", tenMillisecondLimit));
        Assert.IsFalse(limiter.IsLimited("other@test.com", "127.0.0.2", tenMillisecondLimit));
        Assert.IsFalse(limiter.IsLimited("other@else.com", "127.0.0.1", tenMillisecondLimit));
        var stopwatch = Stopwatch.StartNew();
        await limiter.WaitForReleaseAsync("sender@test.com", "127.0.0.1", tenMillisecondLimit);
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }
}
