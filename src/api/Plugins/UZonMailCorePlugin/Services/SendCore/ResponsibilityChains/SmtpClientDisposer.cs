using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

namespace UzonMail.CorePlugin.Services.SendCore.ResponsibilityChains
{
    /// <summary>
    /// SmtpClient 释放
    /// </summary>
    public class SmtpClientDisposer(
        ISmtpClientsManager clientFactory,
        SenderAccountsManager senderAccountsPoolList
    ) : AbstractSendingHandler
    {
        protected override async Task<IHandlerResult> HandleCore(SendingContext context)
        {
            // 不存在或者发件箱待释放时，直接返回
            var senderAccount = context.CurrentAttempt?.PreparedItem.SenderAccount;
            if (senderAccount == null)
                return HandlerResult.Skiped();

            if (!senderAccount.ShouldDispose)
                return HandlerResult.Skiped();

            // 释放发件箱
            var senderAccountKey = new SenderAccountKey(senderAccount.UserId, senderAccount.Id);
            // 仍有可用发件箱时，不释放共享的 SMTP 连接。
            if (!senderAccountsPoolList.ExistValidSenderAccount(senderAccountKey))
                await clientFactory.DisposeSmtpClientsAsync(senderAccountKey);

            return HandlerResult.Success();
        }
    }
}
