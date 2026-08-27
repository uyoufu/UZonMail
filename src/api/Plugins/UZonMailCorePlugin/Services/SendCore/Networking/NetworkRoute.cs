using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Networking;

public enum NetworkRouteKind
{
    Direct,
    StaticProxy,
    DynamicProxy,
}

public sealed record NetworkRoute(
    NetworkRouteKind Kind,
    string Identity,
    ProxyClientAdapter? ProxyClient
)
{
    public static NetworkRoute Direct { get; } = new(NetworkRouteKind.Direct, "direct", null);
}

public sealed record NetworkRouteRequest(
    SenderAccountKey SenderAccount,
    SendingProtocol SendingProtocol,
    string MatchAddress,
    long ExplicitProxyId,
    IReadOnlyList<long> AvailableProxyIds
);

public sealed record NetworkRouteResolution(
    bool IsSuccess,
    NetworkRoute? Route,
    SendFailureKind FailureKind,
    string Message
)
{
    public static NetworkRouteResolution Success(NetworkRoute route) =>
        new(true, route, SendFailureKind.None, string.Empty);

    public static NetworkRouteResolution Failure(string message) =>
        new(false, null, SendFailureKind.Proxy, message);
}

public interface INetworkRouteResolver
{
    Task<NetworkRouteResolution> ResolveAsync(
        NetworkRouteRequest request,
        CancellationToken cancellationToken = default
    );
}
