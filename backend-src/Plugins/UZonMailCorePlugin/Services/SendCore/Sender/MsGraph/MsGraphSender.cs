using log4net;
using MimeKit;
using UZonMail.CorePlugin.Services.Encrypt;
using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.Utils.Results;

namespace UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph
{
    public class MsGraphSender(IServiceProvider provider, EncryptService encryptService)
        : IEmailSender
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MsGraphSender));

        public int Order => 0;

        public bool IsMatch(OutboxType outboxType)
        {
            return outboxType == OutboxType.MsGraph;
        }

        /// <summary>
        /// 开始发送邮件
        /// </summary>
        /// <param name="sendingContext"></param>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public async Task<IHandlerResult> SendAsync(SendingContext context, MimeMessage message)
        {
            SendItemMeta sendItem = context.EmailItem!;
            var outbox = sendItem.Outbox;

            var smtpClientFactory = context.Provider.GetRequiredService<MsGraphClientFactory>();
            var clientResult = await smtpClientFactory.GetMsGraphClientAsync(context);
            // 若返回 null,说明这个发件箱不能建立 smtp 连接，对它进行取消
            if (!clientResult)
            {
                var errorMessage = $"发件箱 {sendItem.Outbox.Email} 错误。{clientResult.Message}";
                _logger.Error(errorMessage);
                // 标记发件箱有问题
                context.OutboxAddress?.MarkShouldDispose(errorMessage);
                return HandlerResult.Failed(errorMessage);
            }
            var client = clientResult.Data;
            try
            {
                // 验证授权并保存 refreshToken
                await client.AuthenticateAsync(
                    outbox.Email,
                    outbox.OutlookClientId,
                    outbox.PlainPassword,
                    outbox.UserId,
                    context.SqlContext
                );
                var sendResult = await client.SendAsync(message);
                _logger.Info(
                    $"邮件发送完成：{sendItem.Outbox.Email} -> {string.Join(",", sendItem.Inboxes.Select(x => x.Email))}"
                );

                // 标记邮件状态
                sendItem.SetStatus(SendItemMetaStatus.Success, sendResult);
                // 标记上下文状态
                return HandlerResult.Success();
            }
            catch (Exception error)
            {
                _logger.Error(error);
                // 发件箱问题，返回失败
                sendItem.Outbox?.MarkShouldDispose(error.Message);
                return HandlerResult.Failed(
                    $"使用发件箱 {sendItem.Outbox?.Email} 发送邮件失败：{error.Message}"
                );
            }
        }

        /// <summary>
        /// 验证发件箱
        /// </summary>
        /// <param name="db"></param>
        /// <param name="outbox">发件箱中的密码是加密后的密码</param>
        /// <returns></returns>
        public async Task<Result<string>> TestOutbox(
            IServiceProvider scopeServiceProvider,
            Outbox outbox
        )
        {
            var client = provider.GetRequiredService<MsGraphClient>();
            client.SetParams(string.Empty, 0);

            var db = scopeServiceProvider.GetRequiredService<SqlContext>();

            // graph 发件不需要代理
            // 若需要，后期增加
            try
            {
                var decryptedPassword = encryptService.DecryptPassword(outbox.Password);
                await client.AuthenticateAsync(
                    outbox.Email,
                    outbox.UserName,
                    decryptedPassword,
                    outbox.UserId,
                    db
                );
                return Result<string>.Success("success");
            }
            catch (Exception e)
            {
                return Result<string>.Fail($"验证发件箱失败：{e.Message}");
            }
        }
    }
}
