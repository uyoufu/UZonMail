using log4net;
using UzonMail.CorePlugin.Services.Encrypt;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

public sealed class SmtpSender(
    EncryptService encryptService,
    IPRateLimiter ipRateLimiter,
    ITransportFailureClassifier failureClassifier,
    SmtpConnector connector
) : IEmailTransport
{
    private const int MaxTransportAttempts = 3;
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SmtpSender));

    public OutboxType Type => OutboxType.SMTP;

    public async Task<TransportResult> SendAsync(
        SendingContext context,
        MimeKit.MimeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var sendItem = context.CurrentAttempt!.PreparedItem;
        var clientManager = context.Provider.GetRequiredService<SmtpClientsManager>();
        TransportResult? lastResult = null;

        for (var attempt = 1; attempt <= MaxTransportAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var routeResolver = context.Provider.GetRequiredService<INetworkRouteResolver>();
            var routeResult = await routeResolver.ResolveAsync(
                new NetworkRouteRequest(
                    new OutboxKey(sendItem.Outbox.UserId, sendItem.Outbox.Id),
                    sendItem.Outbox.OutboxType,
                    sendItem.Outbox.Email,
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

            var clientResult = await clientManager.GetSmtpClientAsync(
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
                var client = clientResult.Data;
                await ipRateLimiter.WaitForReleaseAsync(
                    context,
                    sendItem.Outbox.Email,
                    client.ProxyClient?.ProxyHost
                );

                if (client.ProxyClient is ProxyClientAdapter adapter && !adapter.IsEnable)
                {
                    lastResult = TransportResult.Failure(SendFailureKind.Proxy, "代理在发送前已失效");
                    await clientManager.DisposeSmtpClientAsync(client.GetClientKey());
                }
                else
                {
                    try
                    {
                        var receiptId = await client.SendAsync(message, cancellationToken);
                        Logger.Info(
                            $"邮件发送完成：{sendItem.Outbox.Email} -> {string.Join(",", sendItem.Inboxes.Select(x => x.Email))}"
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
                            && client.ProxyClient is ProxyClientAdapter failedProxy
                        )
                            failedProxy.MarkHealthless();
                        await clientManager.DisposeSmtpClientAsync(client.GetClientKey());
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
        Outbox outbox,
        CancellationToken cancellationToken = default
    )
    {
        var smtpUserName = string.IsNullOrWhiteSpace(outbox.UserName)
            ? outbox.Email
            : outbox.UserName;
        var smtpPassword = encryptService.DecryptPassword(outbox.Password);
        var localError = ValidateParameters(outbox, smtpUserName, smtpPassword);
        if (localError is not null)
            return localError;

        var routeResolver = scopeServiceProvider.GetRequiredService<INetworkRouteResolver>();
        var routeResult = await routeResolver.ResolveAsync(
            new NetworkRouteRequest(
                new OutboxKey(outbox.UserId, outbox.Id),
                outbox.Type,
                outbox.Email,
                outbox.ProxyId,
                []
            ),
            cancellationToken
        );
        if (!routeResult.IsSuccess)
            return TransportResult.Failure(routeResult.FailureKind, routeResult.Message);

        using var client = scopeServiceProvider.GetRequiredService<ThrottlingSmtpClient>();
        client.SetParams(outbox.Email, 0);
        client.ProxyClient = routeResult.Route!.ProxyClient;
        try
        {
            await connector.ConnectAndAuthenticateAsync(
                client,
                new SmtpConnectionProfile(
                    outbox.SmtpHost,
                    outbox.SmtpPort,
                    outbox.ConnectionSecurity.ToMailKitSecureSocketOptions(),
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
        Outbox outbox,
        string? smtpUserName,
        string smtpPassword
    )
    {
        if (string.IsNullOrWhiteSpace(outbox.SmtpHost))
            return TransportResult.Failure(SendFailureKind.LocalData, "SMTP 服务器地址不能为空");
        if (outbox.SmtpPort is <= 0 or > 65535)
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
