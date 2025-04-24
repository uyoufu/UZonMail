using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.Emails;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Core.Services.SendCore.Sender;
using UZonMail.Core.Services.SendCore.WaitList;

namespace UZonMail.Core.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// 本机邮件发送器
    /// </summary>
    public class LocalEmailSender : AbstractSendingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LocalEmailSender));

        protected override async Task HandleCore(SendingContext context)
        {
            _logger.Debug("线程调用本地发件器进行发件");

            // 如果前面失败了，跳过
            if (context.Status.HasFlag(ContextStatus.Fail))
                return;

            var sendItem = context.EmailItem;
            if (sendItem == null)
            {
                return;
            }

            if (!sendItem.Validate(out var status))
            {
                // 数据验证失败，需要移除当前发件项，并标记数据验证失败
                sendItem.SetStatus(SendItemMetaStatus.Error, "发件项数据验证失败，取消发件");
                return;
            }

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
                message.ReplyTo.AddRange(sendItem.ReplyToEmails.Select(x =>
                {
                    return new MailboxAddress(x, x);
                }));
            }
            // 主题
            message.Subject = sendItem.Subject;

            // 正文
            var htmlBody = sendItem.HtmlBody;
            BodyBuilder bodyBuilder = new()
            {
                HtmlBody = htmlBody
            };

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
            var mimeMessageDecorator = context.Provider.GetRequiredService<MimeMessageDecorateService>();
            message = await mimeMessageDecorator.Decorate(emailDecoratorParams, message);

            await SendEmailAndTry(context, message);
        }

        /// <summary>
        /// 发送邮件并重试
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        private static async Task SendEmailAndTry(SendingContext context, MimeMessage message, int tryCount = 3)
        {
            SendItemMeta sendItem = context.EmailItem!;
            var smtpClientFactory = context.Provider.GetRequiredService<SmtpClientFactory>();
            var clientResult = await smtpClientFactory.GetSmtpClientAsync(context);
            // 若返回 null,说明这个发件箱不能建立 smtp 连接，对它进行取消
            if (!clientResult)
            {
                _logger.Error($"发件箱 {sendItem.Outbox.Email} 错误。{clientResult.Message}");
                // 标记发件箱有问题
                context.OutboxAddress?.MarkShouldDispose(clientResult.Message);
                return;
            }

            // throw new NullReferenceException("测试报错");
            var client = clientResult.Data;

            try
            {
                var debugConfig = context.Provider.GetRequiredService<DebugConfig>();
                string sendResult = "测试状态,虚拟发件";
                if (!debugConfig.IsDemo)
                {
                    sendResult = await client.SendAsync(message);
                }

                _logger.Info($"邮件发送完成：{sendItem.Outbox.Email} -> {string.Join(",", sendItem.Inboxes.Select(x => x.Email))}");

                // 标记邮件状态
                sendItem.SetStatus(SendItemMetaStatus.Success, sendResult);
                // 标记上下文状态
                context.Status |= ContextStatus.Success;
            }
            // 错误情况分类
            // 1. 代理刚好失效，导致发生 ServiceNotConnectedException ，从而导致失败
            // 2. 发件箱有问题
            catch (SmtpCommandException smtpCommandException)
            {
                // 收件箱不可达
                if (smtpCommandException.ErrorCode == SmtpErrorCode.RecipientNotAccepted)
                {
                    _logger.Warn(smtpCommandException);
                    sendItem.SetStatus(SendItemMetaStatus.Error, smtpCommandException.Message);
                    return;
                }

                // 发件箱有问题
                sendItem.Outbox?.MarkShouldDispose(smtpCommandException.Message);
                return;
            }
            catch (ServiceNotConnectedException ex)
            {
                // 已经重试多次，说明发件箱有问题
                if (tryCount < 0)
                {
                    _logger.Error(ex);
                    // 发件箱问题，返回失败
                    sendItem.Outbox?.MarkShouldDispose(ex.Message);
                    return;
                }

                // 代理无法连接到服务器
                // 在发件前已经验证过邮件可用，此处应当作是代理出了问题
                // 将当前代理标记为不可用
                if (client.ProxyClient is ProxyClientAdapter clientAdapter)
                {
                    clientAdapter.MarkHealthless();
                }

                await SendEmailAndTry(context, message, tryCount - 1);
            }
            catch (Exception error)
            {
                _logger.Error(error);
                // 发件箱问题，返回失败
                sendItem.Outbox?.MarkShouldDispose(error.Message);
                return;
            }
        }
    }
}
