using System.Security.Authentication;
using log4net;
using MimeKit;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

public sealed class MsGraphSender(
    EncryptService encryptService,
    IMsGraphClientFactory clientFactory
) : IEmailTransport
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(MsGraphSender));

    public OutboxType Type => OutboxType.MsGraph;

    public async Task<TransportResult> SendAsync(
        SendingContext context,
        MimeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var sendItem = context.CurrentAttempt!.PreparedItem;
        var outbox = sendItem.Outbox;
        if (sendItem.EffectiveProxyId > 0 || sendItem.AvailableProxyIds.Count > 0)
            return TransportResult.Failure(SendFailureKind.LocalData, "Outlook Graph 发件不支持代理配置");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var client = clientFactory.GetClient(
                new OutboxKey(outbox.UserId, outbox.Id),
                outbox.Email,
                outbox.OutlookClientId,
                outbox.PlainPassword ?? string.Empty
            );
            await client.AuthenticateAsync(
                outbox.Email,
                outbox.OutlookClientId,
                outbox.PlainPassword ?? string.Empty,
                outbox.Id,
                context.SqlContext
            );
            var receiptId = await client.SendAsync(message);
            return TransportResult.Success(receiptId);
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
            return Classify(exception);
        }
    }

    public async Task<TransportResult> ValidateAsync(
        IServiceProvider scopeServiceProvider,
        Outbox outbox,
        CancellationToken cancellationToken = default
    )
    {
        if (outbox.ProxyId > 0)
            return TransportResult.Failure(SendFailureKind.LocalData, "Outlook Graph 发件不支持代理配置");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var password = encryptService.DecryptPassword(outbox.Password);
            var username = outbox.UserName ?? string.Empty;
            var client = clientFactory.GetClient(
                new OutboxKey(outbox.UserId, outbox.Id),
                outbox.Email,
                username,
                password
            );
            var db = scopeServiceProvider.GetRequiredService<SqlContext>();
            await client.AuthenticateAsync(outbox.Email, username, password, outbox.Id, db);
            return TransportResult.Success();
        }
        catch (Exception exception)
        {
            Logger.Warn(exception);
            return Classify(exception);
        }
    }

    private static TransportResult Classify(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException
                => TransportResult.Failure(SendFailureKind.Cancelled, exception.Message),
            AuthenticationException
                => TransportResult.Failure(
                    SendFailureKind.OutboxPermanent,
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
}
