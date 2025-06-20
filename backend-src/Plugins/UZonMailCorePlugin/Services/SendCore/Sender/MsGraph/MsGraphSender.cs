using log4net;
using MimeKit;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.WaitList;

namespace UZonMail.Core.Services.SendCore.Sender.MsGraph
{
    public class MsGraphSender : IEmailSender
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MsGraphSender));

        public int Order => 0;

        public IAuthenticateClient GetAuthenticateClient()
        {
            return new MsGraphClient(string.Empty, 0);
        }

        public bool IsMatch(string outbox)
        {
            return outbox.Contains("@outlook.com", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 开始发送邮件
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public async Task SendAsync(SendingContext context, MimeMessage message)
        {
            SendItemMeta sendItem = context.EmailItem!;
            var outbox = sendItem.Outbox;

            var client = new MsGraphClient(outbox.Email, 0);            
            try
            {
                await client.AuthenticateAsync(outbox.Email, outbox.AuthUserName, outbox.AuthPassword);

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
