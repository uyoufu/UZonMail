using UzonMail.CorePlugin.Services.SendCore.WaitList;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SendingItemMetaListTests
{
    [TestMethod]
    public void Retry_ReplacesActivePayloadWithLightweightReadyEntry()
    {
        var list = new SendingItemMetaList();
        var original = new SendItemMeta(1, 0, 0);
        Assert.IsTrue(list.Add(original));

        var active = list.GetSendingMeta();
        Assert.AreSame(original, active);
        Assert.AreEqual(0, list.WaitListCount);
        Assert.AreEqual(1, list.ActiveCount);

        Assert.IsTrue(list.Retry(active!));
        var retried = list.GetSendingMeta();
        Assert.IsNotNull(retried);
        Assert.AreNotSame(original, retried);
        Assert.AreEqual(1, retried.TriedCount);
        Assert.IsTrue(list.Complete(retried));
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void SpecificAndSharedQueues_AreIndependentAndUnique()
    {
        var list = new SendingItemMetaList();
        Assert.IsTrue(list.Add(new SendItemMeta(1, 0, 0)));
        Assert.IsTrue(list.Add(new SendItemMeta(2, 20, 0)));
        Assert.IsFalse(list.Add(new SendItemMeta(2, 20, 0)));

        Assert.AreEqual(2L, list.GetSendingMeta(20)!.SendingItemId);
        Assert.IsNull(list.GetSendingMeta(21));
        Assert.AreEqual(1L, list.GetSendingMeta()!.SendingItemId);
    }

    [TestMethod]
    public void RemovePending_DoesNotRemoveAnActiveLease()
    {
        var list = new SendingItemMetaList();
        list.Add(new SendItemMeta(1, 0, 0));
        var active = list.GetSendingMeta()!;

        Assert.IsFalse(list.RemovePendingItem(1));
        Assert.AreEqual(1, list.Count);
        Assert.IsTrue(list.Complete(active));
    }
}
