using log4net;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

public sealed class SmtpSender(
    IPRateLimiter ipRateLimiter,
    ITransportFailureClassifier failureClassifier,
    SmtpConnector connector,
    ISmtpClientsManager clientManager
) : IEmailTransport
{
    private const int MaxTransportAttempts = 3;
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SmtpSender));

    public SendingProtocol Type => SendingProtocol.Smtp;

    public async Task<TransportResult> SendAsync(
        SendingContext context,
        MimeKit.MimeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var sendItem = context.CurrentAttempt!.PreparedItem;
        TransportResult? lastResult = null;

        for (var attempt = 1; attempt <= MaxTransportAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var routeResolver = context.Provider.GetRequiredService<INetworkRouteResolver>();
            var routeResult = await routeResolver.ResolveAsync(
                new NetworkRouteRequest(
                    new SenderAccountKey(sendItem.SenderAccount.UserId, sendItem.SenderAccount.Id),
                    sendItem.SenderAccount.SendingProtocol,
                    sendItem.SenderAccount.Email,
                    sendItem.EffectiveProxyId,
                    sendItem.AvailableProxyIds
                ),
                cancellationToken
            );
            if (!routeResult.IsSuccess || routeResult.Route is null)
            {
                lastResult = TransportResult.Failure(routeResult.FailureKind, routeResult.Message);
                if (attempt == MaxTransportAttempts)
                    return lastResult;
                continue;
            }

            var clientResult = await clientManager.AcquireSmtpSessionAsync(
                context,
                routeResult.Route,
                cancellationToken
            );
            if (!clientResult || clientResult.Data is null)
            {
                var kind =
                    routeResult.Route.Kind != NetworkRouteKind.Direct
                        ? SendFailureKind.Proxy
                        : SendFailureKind.Network;
                lastResult = TransportResult.Failure(kind, clientResult.Message);
                routeResult.Route.ProxyClient?.MarkHealthless();
            }
            else
            {
                await using var clientLease = clientResult.Data;
                await ipRateLimiter.WaitForReleaseAsync(
                    context,
                    sendItem.SenderAccount.Email,
                    clientLease.ProxyClient?.ProxyHost
                );

                if (clientLease.ProxyClient is ProxyClientAdapter adapter && !adapter.IsEnable)
                {
                    lastResult = TransportResult.Failure(SendFailureKind.Proxy, "代理在发送前已失效");
                    clientLease.Invalidate();
                }
                else
                {
                    try
                    {
                        var receiptId = await clientLease.SendMessageAsync(
                            message,
                            cancellationToken
                        );
                        Logger.Info(
                            $"邮件发送完成：{sendItem.SenderAccount.Email} -> {string.Join(",", sendItem.Recipients.Select(x => x.Email))}"
                        );
                        return TransportResult.Success(receiptId);
                    }
                    catch (Exception exception)
                    {
                        lastResult = failureClassifier.Classify(exception);
                        if (
                            lastResult.FailureKind
                                is SendFailureKind.Network
                                    or SendFailureKind.Proxy
                            && clientLease.ProxyClient is ProxyClientAdapter failedProxy
                        )
                            failedProxy.MarkHealthless();
                        clientLease.Invalidate();
                    }
                }
            }

            if (!ShouldRetryTransport(lastResult) || attempt == MaxTransportAttempts)
                return lastResult;
        }

        return lastResult ?? TransportResult.Failure(SendFailureKind.Unknown, "SMTP 发送未返回结果");
    }

    public async Task<TransportResult> ValidateAsync(
        IServiceProvider scopeServiceProvider,
        SenderEmailAddress senderAccount,
        CancellationToken cancellationToken = default
    )
    {
        var smtpUserName = senderAccount.SmtpAuthUserName;
        var smtpPassword = senderAccount.PlainPassword ?? string.Empty;
        var localError = ValidateParameters(senderAccount, smtpUserName, smtpPassword);
        if (localError is not null)
            return localError;

        var routeResolver = scopeServiceProvider.GetRequiredService<INetworkRouteResolver>();
        var routeResult = await routeResolver.ResolveAsync(
            new NetworkRouteRequest(
                new SenderAccountKey(senderAccount.UserId, senderAccount.Id),
                senderAccount.SendingProtocol,
                senderAccount.Email,
                senderAccount.ProxyId,
                []
            ),
            cancellationToken
        );
        if (!routeResult.IsSuccess)
            return TransportResult.Failure(routeResult.FailureKind, routeResult.Message);

        using var client = scopeServiceProvider.GetRequiredService<ThrottlingSmtpClient>();
        client.SetParams(senderAccount.Email, 0);
        client.ProxyClient = routeResult.Route!.ProxyClient;
        try
        {
            await connector.ConnectAndAuthenticateAsync(
                client,
                new SmtpConnectionProfile(
                    senderAccount.SmtpHost,
                    senderAccount.SmtpPort,
                    senderAccount.ConnectionSecurity.ToMailKitSecureSocketOptions(),
                    smtpUserName!,
                    smtpPassword
                ),
                cancellationToken
            );
            return TransportResult.Success(message: $"{smtpUserName} test success");
        }
        catch (Exception exception)
        {
            Logger.Warn(exception);
            return failureClassifier.Classify(exception);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
    }

    private static TransportResult? ValidateParameters(
        SenderEmailAddress senderAccount,
        string? smtpUserName,
        string smtpPassword
    )
    {
        if (string.IsNullOrWhiteSpace(senderAccount.SmtpHost))
            return TransportResult.Failure(SendFailureKind.LocalData, "SMTP 服务器地址不能为空");
        if (senderAccount.SmtpPort is <= 0 or > 65535)
            return TransportResult.Failure(SendFailureKind.LocalData, "SMTP 端口号不正确");
        if (string.IsNullOrWhiteSpace(smtpUserName))
            return TransportResult.Failure(SendFailureKind.LocalData, "SMTP 用户名不能为空");
        if (string.IsNullOrEmpty(smtpPassword))
            return TransportResult.Failure(SendFailureKind.LocalData, "SMTP 密码不能为空");
        return null;
    }

    private static bool ShouldRetryTransport(TransportResult result) =>
        result.FailureKind
            is SendFailureKind.Transient
                or SendFailureKind.Network
                or SendFailureKind.Proxy
                or SendFailureKind.Unknown;
}
