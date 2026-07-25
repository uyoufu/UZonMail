using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.WaitList;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SendItemQueueTests
{
    [TestMethod]
    public void Release_RequeuesTheSameLightweightDescriptor()
    {
        var queue = new SendItemQueue();
        var descriptor = new SendItemDescriptor(1, 10, 0, 0);
        Assert.IsTrue(queue.Add(descriptor));

        var active = queue.AcquireShared();
        Assert.AreSame(descriptor, active);
        Assert.AreEqual(0, queue.ReadyCount);
        Assert.AreEqual(1, queue.ActiveCount);

        Assert.IsTrue(queue.Release(active!));
        Assert.AreSame(descriptor, queue.AcquireShared());
    }

    [TestMethod]
    public void SpecificAndSharedQueues_AreIndependentAndUnique()
    {
        var queue = new SendItemQueue();
        Assert.IsTrue(queue.Add(new SendItemDescriptor(1, 10, 0, 0)));
        Assert.IsTrue(queue.Add(new SendItemDescriptor(2, 10, 20, 0)));
        Assert.IsFalse(queue.Add(new SendItemDescriptor(2, 10, 20, 0)));

        Assert.AreEqual(2L, queue.AcquireSpecific(20)!.Id);
        Assert.IsNull(queue.AcquireSpecific(21));
        Assert.AreEqual(1L, queue.AcquireShared()!.Id);
    }

    [TestMethod]
    public void RemovePending_DoesNotRemoveAnActiveDescriptor()
    {
        var queue = new SendItemQueue();
        queue.Add(new SendItemDescriptor(1, 10, 0, 0));
        var active = queue.AcquireShared()!;

        Assert.IsFalse(queue.RemovePendingItem(1));
        Assert.AreEqual(1, queue.Count);
        Assert.IsTrue(queue.Complete(active));
    }
}
