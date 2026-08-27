using System.Security.Authentication;
using log4net;
using MimeKit;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

public sealed class MsGraphSender(IMsGraphClientFactory clientFactory) : IEmailTransport
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(MsGraphSender));

    public SendingProtocol Type => SendingProtocol.MicrosoftGraph;

    public async Task<TransportResult> SendAsync(
        SendingContext context,
        MimeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var senderAccount = context.CurrentAttempt!.PreparedItem.SenderAccount;
        if (senderAccount.ProxyId > 0)
            return TransportResult.Failure(SendFailureKind.LocalData, "Microsoft Graph 发件不支持代理配置");

        try
        {
            var client = clientFactory.GetClient(senderAccount);
            await client.AuthenticateAsync(senderAccount, context.SqlContext, cancellationToken);
            return TransportResult.Success(await client.SendAsync(message));
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
            return Classify(exception);
        }
    }

    public async Task<TransportResult> ValidateAsync(
        IServiceProvider scopeServiceProvider,
        SenderEmailAddress senderAccount,
        CancellationToken cancellationToken = default
    )
    {
        if (senderAccount.ProxyId > 0)
            return TransportResult.Failure(SendFailureKind.LocalData, "Microsoft Graph 发件不支持代理配置");

        try
        {
            var client = clientFactory.GetClient(senderAccount);
            var db = scopeServiceProvider.GetRequiredService<SqlContext>();
            await client.AuthenticateAsync(senderAccount, db, cancellationToken);
            return TransportResult.Success();
        }
        catch (Exception exception)
        {
            Logger.Warn(exception);
            return Classify(exception);
        }
    }

    private static TransportResult Classify(Exception exception) =>
        exception switch
        {
            OperationCanceledException
                => TransportResult.Failure(SendFailureKind.Cancelled, exception.Message),
            AuthenticationException
                => TransportResult.Failure(
                    SendFailureKind.SenderAccountPermanent,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
            HttpRequestException
            or IOException
            or TimeoutException
                => TransportResult.Failure(
                    SendFailureKind.Network,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
            _
                => TransportResult.Failure(
                    SendFailureKind.Unknown,
                    exception.Message,
                    errorCode: exception.GetType().Name
                ),
        };
}
