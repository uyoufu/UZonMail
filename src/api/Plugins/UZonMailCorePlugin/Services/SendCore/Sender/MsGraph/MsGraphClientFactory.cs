using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.DB.SQL;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

/// <summary>
/// 表示可认证并通过 Microsoft Graph 发送邮件的客户端。
/// </summary>
public interface IMsGraphClient : IEmailSendingClient
{
    Task AuthenticateAsync(
        string email,
        string username,
        string password,
        long outboxId,
        SqlContext db
    );
}

/// <summary>
/// 按发件箱与凭据快照缓存 Microsoft Graph 客户端。
/// </summary>
public interface IMsGraphClientFactory
{
    IMsGraphClient GetClient(
        OutboxKey outbox,
        string email,
        string username,
        string password
    );
}

public sealed class MsGraphClientFactory(IServiceProvider serviceProvider)
    : IMsGraphClientFactory,
        ISingletonService<IMsGraphClientFactory>
{
    private readonly ConcurrentDictionary<MsGraphClientKey, IMsGraphClient> _clients = [];

    public IMsGraphClient GetClient(
        OutboxKey outbox,
        string email,
        string username,
        string password
    )
    {
        var fingerprint = $"{email}\n{username}\n{password}".MD5();
        return _clients.GetOrAdd(
            new MsGraphClientKey(outbox, fingerprint),
            _ =>
            {
                var client = serviceProvider.GetRequiredService<MsGraphClient>();
                client.SetParams(email, 0);
                return client;
            }
        );
    }

    private readonly record struct MsGraphClientKey(OutboxKey Outbox, string ProfileFingerprint);
}
