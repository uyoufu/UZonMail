using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Outboxes;

/// <summary>
/// 验证运行时发件箱的目标集合、额度和释放状态。
/// </summary>
[TestClass]
public sealed class OutboxEmailAddressTests
{
    [TestMethod]
    public void Constructor_DecryptsAndNormalizesOutboxSnapshot()
    {
        var address = SendCoreTestEntityFactory.CreateOutboxAddress(
            configure: outbox =>
            {
                outbox.UserName = string.Empty;
                outbox.Weight = 0;
                outbox.ReplyToEmails = "first@test.com;first@test.com;second@test.com";
                outbox.SentTotalToday = 3;
            }
        );

        Assert.AreEqual("password", address.PlainPassword);
        Assert.AreEqual(address.Email, address.SmtpAuthUserName);
        Assert.AreEqual(1, address.Weight);
        Assert.AreEqual(3, address.SentTotalToday);
        CollectionAssert.AreEquivalent(
            new[] { "first@test.com", "second@test.com" },
            address.ReplyToEmails
        );
    }

    [TestMethod]
    public void SpecificTargets_RequireSpecificFlagAndCanBeRemoved()
    {
        Assert.ThrowsExactly<Exception>(() =>
            SendCoreTestEntityFactory.CreateOutboxAddress(
                type: OutboxEmailAddressType.Shared,
                sendingItemIds: [1]
            )
        );
        var address = SendCoreTestEntityFactory.CreateOutboxAddress(
            type: OutboxEmailAddressType.Specific,
            sendingItemIds: [1, 2, 2]
        );

        CollectionAssert.AreEquivalent(new long[] { 1, 2 }, address.GetSpecificSendingItemIds());
        address.RemoveSepecificSendingItem(10, 1);
        CollectionAssert.AreEqual(new long[] { 2 }, address.GetSpecificSendingItemIds(10));
        address.RemoveSendingGroup(10);
        Assert.IsFalse(address.IsWorking);
    }

    [TestMethod]
    public void Update_MergesTypesAndTargetsWithoutDuplicates()
    {
        var shared = SendCoreTestEntityFactory.CreateOutboxAddress();
        var specific = SendCoreTestEntityFactory.CreateOutboxAddress(
            type: OutboxEmailAddressType.Specific,
            sendingItemIds: [5, 6],
            configure: outbox =>
            {
                outbox.Weight = 9;
                outbox.ReplyToEmails = "updated@test.com";
            }
        );

        shared.Update(specific);

        Assert.IsTrue(shared.Type.HasFlag(OutboxEmailAddressType.Shared));
        Assert.IsTrue(shared.Type.HasFlag(OutboxEmailAddressType.Specific));
        Assert.AreEqual(9, shared.Weight);
        CollectionAssert.AreEqual(new long[] { 5, 6 }, shared.GetSpecificSendingItemIds());
    }

    [TestMethod]
    public void DisposalAndTaskMarkers_AreIdempotent()
    {
        var address = SendCoreTestEntityFactory.CreateOutboxAddress();

        Assert.IsTrue(address.TryMarkTaskRunning());
        Assert.IsFalse(address.TryMarkTaskRunning());
        Assert.IsTrue(address.IsRunningInTask);
        address.MarkTaskStopped();
        Assert.IsFalse(address.IsRunningInTask);

        address.MarkInvalid("invalid credentials");
        Assert.IsTrue(address.ShouldDispose);
        Assert.IsTrue(address.IsPermanentlyInvalid);
        Assert.IsFalse(address.Enable);
        Assert.AreEqual("invalid credentials", address.ErroredMessage);
    }

    [TestMethod]
    public void SendingTargetId_UsesBothGroupAndItemForEquality()
    {
        var first = new SendingTargetId(10, 20);
        var equal = new SendingTargetId(10, 20);
        var different = new SendingTargetId(10, 21);

        Assert.AreEqual(first, equal);
        Assert.AreEqual(first.GetHashCode(), equal.GetHashCode());
        Assert.AreNotEqual(first, different);
        Assert.IsFalse(first.Equals("10_20"));
    }
}
