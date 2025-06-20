using log4net;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Core.Services.SendCore.WaitList;

namespace UZonMail.Core.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// 本机邮件发送器
    /// </summary>
    public class SmtpSender : IEmailSender
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmtpSender));

        public int Order => int.MaxValue;

        public bool IsMatch(string outbox)
        {
            return true;
        }

        public IAuthenticateClient GetAuthenticateClient()
        {
            return new ThrottlingSmtpClient(string.Empty, 0);
        }

        public async Task SendAsync(SendingContext context, MimeMessage message)
        {
            await SendAsync(context, message, 3);
        }

        private static async Task SendAsync(SendingContext context, MimeMessage message, int tryCount)
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

                await SendAsync(context, message, tryCount - 1);
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
