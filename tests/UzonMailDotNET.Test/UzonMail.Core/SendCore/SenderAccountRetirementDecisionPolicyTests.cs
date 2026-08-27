using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class SenderAccountRetirementDecisionPolicyTests
{
    [TestMethod]
    public void NoCurrentItem_DoesNotAffectItemState()
    {
        Assert.AreEqual(
            CurrentSendItemDisposition.None,
            SenderAccountRetirementDecisionPolicy.Decide(null, false)
        );
    }

    [TestMethod]
    public void SpecificItem_AlwaysFailsWhenItsSenderAccountRetires()
    {
        var descriptor = new SendItemDescriptor(1, 10, 20, 0);
        Assert.AreEqual(
            CurrentSendItemDisposition.Fail,
            SenderAccountRetirementDecisionPolicy.Decide(descriptor, true)
        );
    }

    [TestMethod]
    public void SharedItem_RetriesOnlyWhenAnotherSenderAccountExists()
    {
        var descriptor = new SendItemDescriptor(1, 10, 0, 0);
        Assert.AreEqual(
            CurrentSendItemDisposition.RetryWithAnotherSenderAccount,
            SenderAccountRetirementDecisionPolicy.Decide(descriptor, true)
        );
        Assert.AreEqual(
            CurrentSendItemDisposition.Fail,
            SenderAccountRetirementDecisionPolicy.Decide(descriptor, false)
        );
    }
}
