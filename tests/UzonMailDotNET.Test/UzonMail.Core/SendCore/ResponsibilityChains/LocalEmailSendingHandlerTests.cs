using MimeKit;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.ResponsibilityChains;

/// <summary>
/// 验证发送层构造的 MIME 正文可为 HTML 客户端和纯文本客户端提供一致内容。
/// </summary>
[TestClass]
public sealed class LocalEmailSendingHandlerTests
{
    private const string HtmlBody = """
        <!doctype html>
        <html lang="zh-CN">
        <head>
          <title>不应出现在正文</title>
          <style>.hidden { display:none; }</style>
        </head>
        <body>
          <p>致：test@example.com，您好！</p>
          <p>请添加客服微信。</p>
          <script>window.hidden = true;</script>
          <img src="https://example.test/contact.png" alt="客服二维码">
        </body>
        </html>
        """;

    [TestMethod]
    public void CreateMessageBody_CreatesEquivalentPlainTextAlternative()
    {
        var body = LocalEmailSendingHandler.CreateMessageBody(HtmlBody, []);
        var message = new MimeMessage { Body = body };

        Assert.IsInstanceOfType<MultipartAlternative>(body);
        Assert.AreEqual(NormalizeLineEndings(HtmlBody), NormalizeLineEndings(message.HtmlBody));
        Assert.IsNotNull(message.TextBody);
        StringAssert.Contains(message.TextBody, "致：test@example.com，您好！");
        StringAssert.Contains(message.TextBody, "请添加客服微信。");
        StringAssert.Contains(message.TextBody, "客服二维码");
        Assert.DoesNotContain("不应出现在正文", message.TextBody);
        Assert.DoesNotContain("window.hidden", message.TextBody);
    }

    [TestMethod]
    public void CreateMessageBody_NestsAlternativesWhenAttachmentsExist()
    {
        var attachmentPath = Path.GetTempFileName();
        File.WriteAllText(attachmentPath, "attachment content");
        try
        {
            IReadOnlyList<PreparedSendAttachment> attachments =
            [
                new PreparedSendAttachment("readme.txt", new FileInfo(attachmentPath)),
            ];

            var body = LocalEmailSendingHandler.CreateMessageBody(HtmlBody, attachments);
            var message = new MimeMessage { Body = body };

            var mixedBody = body as Multipart;
            Assert.IsNotNull(mixedBody);
            Assert.AreEqual("mixed", mixedBody.ContentType.MediaSubtype);
            Assert.IsInstanceOfType<MultipartAlternative>(mixedBody[0]);
            Assert.AreEqual(NormalizeLineEndings(HtmlBody), NormalizeLineEndings(message.HtmlBody));
            Assert.IsNotNull(message.TextBody);
            Assert.AreEqual(1, message.Attachments.Count());
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [TestMethod]
    public void CreateMessageBody_ExtractsBlockContentAndDecodesHtmlEntities()
    {
        const string htmlBody = """
            <html>
              <body>
                <div>First&nbsp;line<br>Second &amp; third</div>
                <p>Final paragraph</p>
              </body>
            </html>
            """;
        var body = LocalEmailSendingHandler.CreateMessageBody(htmlBody, []);
        var message = new MimeMessage { Body = body };

        Assert.AreEqual(
            string.Join(Environment.NewLine, ["First line", "Second & third", "Final paragraph"]),
            message.TextBody
        );
    }

    private static string NormalizeLineEndings(string? content) =>
        (content ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
}
