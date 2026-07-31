using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using log4net;
using MimeKit;
using UzonMail.CorePlugin.Services.EmailDecorator;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Sender;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains;

/// <summary>
/// 构造 MIME 消息并调用匹配的邮件 Transport。
/// </summary>
public sealed class LocalEmailSendingHandler(
    EmailSendersManager sendersManager,
    PreparedSendItemValidator validator
) : AbstractSendingHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalEmailSendingHandler));
    private static readonly Regex WhitespacePattern = new(@"\s+", RegexOptions.Compiled);
    private static readonly HashSet<string> IgnoredElementTagNames =
        new(StringComparer.OrdinalIgnoreCase) { "head", "script", "style", "noscript" };
    private static readonly HashSet<string> BlockElementTagNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "address",
            "article",
            "blockquote",
            "div",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "li",
            "p",
            "table",
            "tr",
        };

    protected override async Task<IHandlerResult> HandleCore(SendingContext context)
    {
        if (context.IsFailed())
            return HandlerResult.Skiped();

        var currentAttempt = context.CurrentAttempt;
        if (currentAttempt == null)
            return HandlerResult.Skiped();

        var validation = validator.Validate(currentAttempt.PreparedItem);
        if (!validation.IsValid)
        {
            Logger.Warn($"数据验证失败: 发件项 {currentAttempt.Descriptor.Id} {validation.Message}");
            context.TransportResult = TransportResult.Failure(
                SendFailureKind.LocalData,
                $"发件项数据验证失败：{validation.Message}"
            );
            return HandlerResult.Failed(context.TransportResult.Message);
        }

        var message = await CreateMimeMessageAsync(context, currentAttempt.PreparedItem);
        var transport = sendersManager.GetEmailSender(
            currentAttempt.PreparedItem.Outbox.OutboxType
        );
        var result = await transport.SendAsync(context, message);
        context.TransportResult = result;

        if (result.FailureKind == SendFailureKind.OutboxPermanent)
            currentAttempt.PreparedItem.Outbox.MarkInvalid(result.Message);

        return result switch
        {
            { IsSuccess: true } => HandlerResult.Success(result.Message),
            { FailureKind: SendFailureKind.Cancelled } => HandlerResult.Skiped(result.Message),
            _ => HandlerResult.Failed(result.Message),
        };
    }

    private static async Task<MimeMessage> CreateMimeMessageAsync(
        SendingContext context,
        PreparedSendItem item
    )
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(item.Outbox.Name, item.Outbox.Email));
        message.To.AddRange(
            item.Inboxes.Where(x => !string.IsNullOrEmpty(x.Email))
                .Select(x => new MailboxAddress(x.Name, x.Email))
        );
        message.Cc.AddRange(
            item.CC.Where(x => !string.IsNullOrEmpty(x.Email))
                .Select(x => new MailboxAddress(x.Name, x.Email))
        );
        message.Bcc.AddRange(
            item.BCC.Where(x => !string.IsNullOrEmpty(x.Email))
                .Select(x => new MailboxAddress(x.Name, x.Email))
        );
        message.ReplyTo.AddRange(item.ReplyToEmails.Select(x => new MailboxAddress(x, x)));
        message.Subject = item.Subject;

        message.Body = CreateMessageBody(item.HtmlBody, item.Attachments);

        var decoratorParams = new EmailDecoratorParams(
            item.SendingSetting,
            item.SourceItem,
            item.Variables,
            item.Outbox.Outbox,
            item.Subject,
            item.HtmlBody
        );
        return await context
            .Provider.GetRequiredService<MimeMessageDecorateService>()
            .Decorate(decoratorParams, message);
    }

    /// <summary>
    /// 根据最终 HTML 和附件构造规范 MIME 正文。
    /// 有可见文本时同时提供纯文本替代部分，避免接收方将 HTML 邮件识别为替代正文不一致。
    /// </summary>
    internal static MimeEntity CreateMessageBody(
        string htmlBody,
        IReadOnlyList<PreparedSendAttachment> attachments
    )
    {
        ArgumentNullException.ThrowIfNull(htmlBody);
        ArgumentNullException.ThrowIfNull(attachments);

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        var plainTextBody = CreatePlainTextAlternative(htmlBody);
        if (!string.IsNullOrWhiteSpace(plainTextBody))
            bodyBuilder.TextBody = plainTextBody;

        foreach (var attachment in attachments)
        {
            bodyBuilder.Attachments.Add(attachment.File.FullName);
            var mimeAttachment = bodyBuilder.Attachments.Last();
            mimeAttachment.ContentType.Name = attachment.FileName;
            mimeAttachment.ContentDisposition?.FileName = attachment.FileName;
        }

        return bodyBuilder.ToMessageBody();
    }

    private static string CreatePlainTextAlternative(string htmlBody)
    {
        var htmlDocument = new HtmlParser().ParseDocument(htmlBody);
        var contentElement = htmlDocument.Body ?? htmlDocument.DocumentElement;
        if (contentElement is null)
            return string.Empty;

        var plainTextBuilder = new StringBuilder();
        AppendPlainText(contentElement, plainTextBuilder);

        var normalizedText = plainTextBuilder.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
        return string.Join(
            Environment.NewLine,
            normalizedText
                .Split('\n')
                .Select(line => WhitespacePattern.Replace(line, " ").Trim())
                .Where(line => !string.IsNullOrEmpty(line))
        );
    }

    /// <summary>
    /// 按 HTML 文档顺序收集可读文本，使纯文本部分与 HTML 正文保持语义一致。
    /// </summary>
    private static void AppendPlainText(INode node, StringBuilder plainTextBuilder)
    {
        if (node is IText textNode)
        {
            plainTextBuilder.Append(textNode.TextContent);
            return;
        }

        if (node is not IElement element)
            return;

        if (IgnoredElementTagNames.Contains(element.LocalName))
            return;

        if (element.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase))
        {
            plainTextBuilder.Append('\n');
            return;
        }

        if (element.LocalName.Equals("img", StringComparison.OrdinalIgnoreCase))
        {
            plainTextBuilder.Append(element.GetAttribute("alt")?.Trim());
            return;
        }

        var isBlockElement = BlockElementTagNames.Contains(element.LocalName);
        if (isBlockElement)
            plainTextBuilder.Append('\n');

        foreach (var childNode in element.ChildNodes)
            AppendPlainText(childNode, plainTextBuilder);

        if (isBlockElement)
            plainTextBuilder.Append('\n');
    }
}
