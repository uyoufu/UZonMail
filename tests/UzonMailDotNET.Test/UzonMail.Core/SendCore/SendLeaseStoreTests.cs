using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Runtime;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SendLeaseStoreTests
{
    [TestMethod]
    public void Lease_IsUniqueUntilCompleted()
    {
        var store = new InMemorySendLeaseStore();
        var now = DateTimeOffset.UtcNow;
        var item = new SendItemDescriptor(1, 10, 0, 0);
        var senderAccount = new SenderAccountKey(20, 30);

        Assert.IsTrue(
            store.TryAcquire(item, senderAccount, now, TimeSpan.FromMinutes(1), out var lease)
        );
        Assert.IsFalse(store.TryAcquire(item, senderAccount, now, TimeSpan.FromMinutes(1), out _));
        Assert.IsTrue(store.TryComplete(lease.LeaseId, now.AddSeconds(1), out var completed));
        Assert.AreEqual(SendLeaseState.Completed, completed!.State);
        Assert.IsTrue(
            store.TryAcquire(item, senderAccount, now.AddSeconds(2), TimeSpan.FromMinutes(1), out _)
        );
    }

    [TestMethod]
    public void ExpiredLease_RejectsLateCompletionAndCanBeReacquired()
    {
        var store = new InMemorySendLeaseStore();
        var now = DateTimeOffset.UtcNow;
        var item = new SendItemDescriptor(1, 10, 0, 0);
        var senderAccount = new SenderAccountKey(20, 30);

        Assert.IsTrue(
            store.TryAcquire(item, senderAccount, now, TimeSpan.FromSeconds(5), out var lease)
        );
        var expired = store.ReclaimExpired(now.AddSeconds(5));

        Assert.HasCount(1, expired);
        Assert.AreEqual(SendLeaseState.Expired, expired[0].State);
        Assert.IsFalse(store.TryComplete(lease.LeaseId, now.AddSeconds(6), out _));
        Assert.IsTrue(
            store.TryAcquire(item, senderAccount, now.AddSeconds(6), TimeSpan.FromSeconds(5), out _)
        );
    }
}
