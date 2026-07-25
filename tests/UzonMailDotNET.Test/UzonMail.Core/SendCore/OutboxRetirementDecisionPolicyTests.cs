using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class OutboxRetirementDecisionPolicyTests
{
    [TestMethod]
    public void NoCurrentItem_DoesNotAffectItemState()
    {
        Assert.AreEqual(
            CurrentSendItemDisposition.None,
            OutboxRetirementDecisionPolicy.Decide(null, false)
        );
    }

    [TestMethod]
    public void SpecificItem_AlwaysFailsWhenItsOutboxRetires()
    {
        var descriptor = new SendItemDescriptor(1, 10, 20, 0);
        Assert.AreEqual(
            CurrentSendItemDisposition.Fail,
            OutboxRetirementDecisionPolicy.Decide(descriptor, true)
        );
    }

    [TestMethod]
    public void SharedItem_RetriesOnlyWhenAnotherOutboxExists()
    {
        var descriptor = new SendItemDescriptor(1, 10, 0, 0);
        Assert.AreEqual(
            CurrentSendItemDisposition.RetryWithAnotherOutbox,
            OutboxRetirementDecisionPolicy.Decide(descriptor, true)
        );
        Assert.AreEqual(
            CurrentSendItemDisposition.Fail,
            OutboxRetirementDecisionPolicy.Decide(descriptor, false)
        );
    }
}
