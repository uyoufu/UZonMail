using MailKit.Net.Imap;
using UzonMail.CorePlugin.Services.EmailReceiving;

namespace UzonMailDotNET.Test.UzonMail.Core.EmailReceiving;

/// <summary>
/// 验证 IMAP RFC 2971 客户端身份声明规则。
/// </summary>
[TestClass]
public sealed class ImapClientIdentificationTests
{
    [TestMethod]
    public void SupportsIdentification_RequiresIdCapability()
    {
        Assert.IsTrue(
            ImapClientIdentification.SupportsIdentification(
                ImapCapabilities.Idle | ImapCapabilities.Id
            )
        );
        Assert.IsFalse(ImapClientIdentification.SupportsIdentification(ImapCapabilities.Idle));
    }

    [TestMethod]
    public void CreateClientImplementation_ProvidesProviderCompatibleIdentity()
    {
        var implementation = ImapClientIdentification.CreateClientImplementation();

        Assert.AreEqual("UzonMail", implementation.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(implementation.Version));
        Assert.AreEqual("UzonMail", implementation.Vendor);
        Assert.AreEqual("https://github.com/uyoufu/UzonMail", implementation.SupportUrl);
        Assert.AreEqual("uyoufu@uzoncloud.com", implementation.Properties["support-email"]);
        Assert.IsNull(implementation.OS);
        Assert.IsNull(implementation.OSVersion);
    }
}
