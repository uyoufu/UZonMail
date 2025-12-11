using log4net;
using MimeKit;
using UZonMail.CorePlugin.Services.EmailDecorator;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Sender;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 本机邮件发送器
    /// </summary>
    public class LocalEmailSendingHandler(EmailSendersManager sendersManager)
        : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            typeof(LocalEmailSendingHandler)
        );

        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            _logger.Debug("线程调用本地发件器进行发件");

            // 如果前面失败了，跳过
            if (context.IsFailed())
                return HandlerResult.Skiped();

            var sendItem = context.EmailItem;
            if (sendItem == null)
            {
                return HandlerResult.Skiped();
            }

            if (!sendItem.Validate(out var status))
            {
                // 数据验证失败，需要移除当前发件项，并标记数据验证失败
                sendItem.SetStatus(SendItemMetaStatus.Error, "发件项数据验证失败，取消发件");
                return HandlerResult.Skiped();
            }

            var mimeMessage = await CreateMimeMessage(context);

            // 调用发件器进行发件
            var emailSender = sendersManager.GetEmailSender(
                sendItem.Outbox.Email,
                sendItem.Outbox.SmtpHost
            );
            if (emailSender == null)
            {
                _logger.Error($"没有找到匹配的邮件发送器，发件箱：{sendItem.Outbox.Email}");
                sendItem.SetStatus(SendItemMetaStatus.Error, "没有找到匹配的邮件发送器");
                return HandlerResult.Skiped();
            }

            return await emailSender.SendAsync(context, mimeMessage);
        }

        private static async Task<MimeMessage> CreateMimeMessage(SendingContext context)
        {
            var sendItem = context.EmailItem!;

            // 参考：https://github.com/jstedfast/MailKit/tree/master/Documentation/Examples
            // 本机发件逻辑
            var message = new MimeMessage();
            // 发件人
            message.From.Add(new MailboxAddress(sendItem.Outbox.Name, sendItem.Outbox.Email));
            // 收件人、抄送、密送
            foreach (var address in sendItem.Inboxes)
            {
                if (string.IsNullOrEmpty(address.Email))
                    continue;
                message.To.Add(new MailboxAddress(address.Name, address.Email));
            }
            if (sendItem.CC != null)
                foreach (var address in sendItem.CC)
                {
                    if (string.IsNullOrEmpty(address.Email))
                        continue;
                    message.Cc.Add(new MailboxAddress(address.Name, address.Email));
                }
            if (sendItem.BCC != null)
                foreach (var address in sendItem.BCC)
                {
                    if (string.IsNullOrEmpty(address.Email))
                        continue;
                    message.Bcc.Add(new MailboxAddress(address.Name, address.Email));
                }
            // 回信人
            if (sendItem.ReplyToEmails.Count > 0)
            {
                message.ReplyTo.AddRange(
                    sendItem.ReplyToEmails.Select(x =>
                    {
                        return new MailboxAddress(x, x);
                    })
                );
            }
            // 主题
            message.Subject = sendItem.Subject;

            // 正文
            var htmlBody = sendItem.HtmlBody;
            BodyBuilder bodyBuilder = new() { HtmlBody = htmlBody };

            // 附件
            var attachments = sendItem.Attachments;
            foreach (var attachment in attachments)
            {
                // 添加附件
                bodyBuilder.Attachments.Add(attachment.FullName);
                // 修改文件名
                var lastOne = bodyBuilder.Attachments.Last();
                lastOne.ContentType.Name = attachment.Name;
                lastOne.ContentDisposition.FileName = attachment.Name;
            }
            message.Body = bodyBuilder.ToMessageBody();

            // 对 message 进行额外的设置
            var emailDecoratorParams = await sendItem.GetEmailDecoratorParams(context);
            var mimeMessageDecorator =
                context.Provider.GetRequiredService<MimeMessageDecorateService>();
            message = await mimeMessageDecorator.Decorate(emailDecoratorParams, message);
            return message;
        }
    }
}
