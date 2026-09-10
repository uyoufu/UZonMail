using UzonMail.CorePlugin.Services.MailConversations;

namespace UzonMailDotNET.Test.UzonMail.Core.MailConversations;

/// <summary>
/// 验证列表摘要始终是安全且受限长度的纯文本。
/// </summary>
[TestClass]
public sealed class MailMessagePreviewFormatterTests
{
    [TestMethod]
    public void Normalize_StripsTagsDecodesEntitiesAndCollapsesWhitespace()
    {
        var preview = MailMessagePreviewFormatter.Normalize(
            "<p>Hello&nbsp; <strong>mail</strong></p>\n<p>next</p>"
        );

        Assert.AreEqual("Hello mail next", preview);
    }

    [TestMethod]
    public void Normalize_TruncatesPreviewToConfiguredMaximumLength()
    {
        var preview = MailMessagePreviewFormatter.Normalize(
            new string('a', MailMessagePreviewFormatter.MaximumLength + 1)
        );

        Assert.IsNotNull(preview);
        Assert.AreEqual(MailMessagePreviewFormatter.MaximumLength, preview.Length);
    }
}
