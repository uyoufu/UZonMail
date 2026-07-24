using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

public sealed class MsGraphClientFactory(IServiceProvider serviceProvider) : ISingletonService
{
    private readonly ConcurrentDictionary<MsGraphClientKey, MsGraphClient> _clients = [];

    public MsGraphClient GetClient(OutboxKey outbox, string email, string username, string password)
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
