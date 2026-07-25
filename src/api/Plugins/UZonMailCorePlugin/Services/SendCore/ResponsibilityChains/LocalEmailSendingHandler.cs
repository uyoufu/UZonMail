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

        var bodyBuilder = new BodyBuilder { HtmlBody = item.HtmlBody };
        foreach (var attachment in item.Attachments)
        {
            bodyBuilder.Attachments.Add(attachment.File.FullName);
            var mimeAttachment = bodyBuilder.Attachments.Last();
            mimeAttachment.ContentType.Name = attachment.FileName;
            if (mimeAttachment.ContentDisposition is not null)
                mimeAttachment.ContentDisposition.FileName = attachment.FileName;
        }
        message.Body = bodyBuilder.ToMessageBody();

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
}
