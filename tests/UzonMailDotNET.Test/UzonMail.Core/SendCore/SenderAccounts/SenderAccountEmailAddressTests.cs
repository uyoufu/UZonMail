using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.SenderAccounts;

/// <summary>
/// 验证运行时发件箱的目标集合、额度和释放状态。
/// </summary>
[TestClass]
public sealed class SenderEmailAddressTests
{
    [TestMethod]
    public void Constructor_NormalizesSenderSnapshot()
    {
        var address = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            configure: senderAccount =>
            {
                senderAccount.ReplyToEmails = "first@test.com;first@test.com;second@test.com";
                senderAccount.SentTotalToday = 3;
            }
        );

        Assert.AreEqual("password", address.PlainPassword);
        Assert.AreEqual(address.Email, address.SmtpAuthUserName);
        Assert.AreEqual(3, address.SentTotalToday);
        CollectionAssert.AreEquivalent(
            new[] { "first@test.com", "second@test.com" },
            address.ReplyToEmails
        );
    }

    [TestMethod]
    public void SpecificTargets_RequireSpecificFlagAndCanBeRemoved()
    {
        Assert.ThrowsExactly<Exception>(
            () =>
                SendCoreTestEntityFactory.CreateSenderAccountAddress(
                    type: SenderEmailAddressType.Shared,
                    sendingItemIds: [1]
                )
        );
        var address = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            type: SenderEmailAddressType.Specific,
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
        var shared = SendCoreTestEntityFactory.CreateSenderAccountAddress();
        var specific = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            type: SenderEmailAddressType.Specific,
            sendingItemIds: [5, 6],
            configure: senderAccount =>
            {
                senderAccount.ReplyToEmails = "updated@test.com";
            }
        );

        shared.Update(specific);

        Assert.IsTrue(shared.Type.HasFlag(SenderEmailAddressType.Shared));
        Assert.IsTrue(shared.Type.HasFlag(SenderEmailAddressType.Specific));
        CollectionAssert.AreEqual(new long[] { 5, 6 }, shared.GetSpecificSendingItemIds());
    }

    [TestMethod]
    public void DisposalAndTaskMarkers_AreIdempotent()
    {
        var address = SendCoreTestEntityFactory.CreateSenderAccountAddress();

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

    [TestMethod]
    public void Update_KeepsSharedAndSpecificBindingsIsolatedBySendingGroup()
    {
        var address = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            sendingGroupId: 10,
            type: SenderEmailAddressType.Shared
        );
        var otherGroupBinding = SendCoreTestEntityFactory.CreateSenderAccountAddress(
            sendingGroupId: 20,
            type: SenderEmailAddressType.Specific,
            sendingItemIds: [5]
        );

        address.Update(otherGroupBinding);

        Assert.AreEqual(SenderEmailAddressType.Shared, address.GetTypeForSendingGroup(10));
        Assert.AreEqual(SenderEmailAddressType.Specific, address.GetTypeForSendingGroup(20));
        Assert.AreEqual(SenderEmailAddressType.None, address.GetTypeForSendingGroup(30));
    }

    [TestMethod]
    public void CoolingAndQuotaBlockedSenderAccount_IsNotEligibleUntilBothDeadlinesPass()
    {
        var address = SendCoreTestEntityFactory.CreateSenderAccountAddress();
        var utcNow = new DateTimeOffset(2026, 7, 25, 10, 0, 0, TimeSpan.Zero);

        address.ScheduleCooldown(TimeSpan.FromSeconds(20), utcNow);
        address.ScheduleDailyQuotaReset(utcNow);

        Assert.IsFalse(address.IsEligible(utcNow.AddSeconds(20)));
        Assert.IsTrue(address.IsQuotaBlocked(utcNow.AddHours(1)));
        Assert.AreEqual(
            new DateTimeOffset(2026, 7, 26, 0, 0, 0, TimeSpan.Zero),
            address.NextEligibleUtc
        );
        Assert.IsTrue(address.IsEligible(address.NextEligibleUtc));
    }

    [TestMethod]
    public void Manager_GroupIndexKeepsSenderAccountUntilItsLastGroupIsRemoved()
    {
        var manager = new SenderAccountsManager();
        manager.AddSenderAccount(
            SendCoreTestEntityFactory.CreateSenderAccountAddress(
                sendingGroupId: 10,
                type: SenderEmailAddressType.Shared
            )
        );
        manager.AddSenderAccount(
            SendCoreTestEntityFactory.CreateSenderAccountAddress(
                sendingGroupId: 20,
                type: SenderEmailAddressType.Shared
            )
        );

        Assert.IsTrue(manager.ExistValidSenderAccount(10));
        Assert.IsTrue(manager.ExistValidSenderAccount(20));
        Assert.IsEmpty(manager.RemoveSenderAccount(10, "group completed"));
        Assert.IsFalse(manager.ExistValidSenderAccount(10));
        Assert.IsTrue(manager.ExistValidSenderAccount(20));
        Assert.HasCount(1, manager.RemoveSenderAccount(20, "group completed"));
        Assert.AreEqual(0, manager.Count);
    }
}
