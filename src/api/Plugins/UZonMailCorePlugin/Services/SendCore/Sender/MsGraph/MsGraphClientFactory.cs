using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;
using UzonMail.DB.SQL;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;

public interface IMsGraphClient : IEmailSendingClient
{
    Task AuthenticateAsync(
        SenderEmailAddress senderAccount,
        SqlContext db,
        CancellationToken cancellationToken = default
    );
}

public interface IMsGraphClientFactory
{
    IMsGraphClient GetClient(SenderEmailAddress senderAccount);
}

public sealed class MsGraphClientFactory(IServiceProvider serviceProvider)
    : IMsGraphClientFactory,
        ISingletonService<IMsGraphClientFactory>
{
    private readonly ConcurrentDictionary<MsGraphClientKey, IMsGraphClient> _clients = [];

    public IMsGraphClient GetClient(SenderEmailAddress senderAccount)
    {
        var oauth = senderAccount.Credentials.OAuth;
        var fingerprint =
            $"{senderAccount.Email}\n{oauth?.ClientId}\n{oauth?.RefreshToken}\n{oauth?.AuthorizedScopes}".MD5();
        return _clients.GetOrAdd(
            new MsGraphClientKey(
                new SenderAccountKey(senderAccount.UserId, senderAccount.Id),
                fingerprint
            ),
            _ =>
            {
                var client = serviceProvider.GetRequiredService<MsGraphClient>();
                client.SetParams(senderAccount.Email, 0);
                return client;
            }
        );
    }

    private readonly record struct MsGraphClientKey(
        SenderAccountKey SenderAccount,
        string ProfileFingerprint
    );
}
