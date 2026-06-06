using UZonMail.CorePlugin.Services.SendCore.Contexts;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.Sender.Smtp;

namespace UZonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// SmtpClient 释放
    /// </summary>
    public class SmtpClientDisposer(
        SmtpClientsManager clientFactory,
        OutboxesManager outboxesPoolList
    ) : AbstractSendingHandler
    {
        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 不存在或者发件箱待释放时，直接返回
            var outbox = context.EmailItem?.Outbox;
            if (outbox == null)
                return HandlerResult.Skiped();

            if (!outbox.ShouldDispose)
                return HandlerResult.Skiped();

            // 释放发件箱
            var keys = clientFactory.SmtpClientKeys.Where(x => x.Email == outbox.Email).ToList();
            foreach (var key in keys)
            {
                // 仍有可用发件箱时，不释放共享的 SMTP 连接
                if (outboxesPoolList.ExistValidOutbox(key.Email))
                    continue;
                await clientFactory.DisposeSmtpClientAsync(key);
            }

            return HandlerResult.Success();
        }
    }
}
