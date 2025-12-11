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
                // 判断是否存在，若存在，则不释放
                if (!outboxesPoolList.ExistValidOutbox(key.Email))
                    continue;
                clientFactory.DisposeSmtpClient(key);
            }

            return HandlerResult.Success();
        }
    }
}
