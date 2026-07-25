using UzonMail.DB.Extensions;

namespace UzonMailDotNET.Test.UzonMail.DB.Extensions;

[TestClass]
public sealed class ObjectCopyExtensionsTests
{
    [TestMethod]
    public void CopyAllProperties_SkipsTargetPropertiesMissingFromSourceRuntimeType()
    {
        CopyTarget target = new ExtendedCopyTarget
        {
            Name = "before",
            ExtendedValue = "preserved",
        };
        CopyTarget source = new() { Name = "after" };

        target.CopyAllProperties(source);

        Assert.AreEqual("after", target.Name);
        Assert.AreEqual("preserved", ((ExtendedCopyTarget)target).ExtendedValue);
    }

    private class CopyTarget
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ExtendedCopyTarget : CopyTarget
    {
        public string ExtendedValue { get; set; } = string.Empty;
    }
}
