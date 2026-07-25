using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Networking;

public sealed class NetworkRouteResolver(
    IServiceProvider serviceProvider,
    IProxiesManager proxiesManager
) : INetworkRouteResolver, IScopedService<INetworkRouteResolver>
{
    public async Task<NetworkRouteResolution> ResolveAsync(
        NetworkRouteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasProxy = request.ExplicitProxyId > 0 || request.AvailableProxyIds.Count > 0;
        if (request.OutboxType == OutboxType.MsGraph)
        {
            return hasProxy
                ? NetworkRouteResolution.Failure("Outlook Graph 发件不支持代理配置")
                : NetworkRouteResolution.Success(NetworkRoute.Direct);
        }

        if (!hasProxy)
            return NetworkRouteResolution.Success(NetworkRoute.Direct);

        var handler = await proxiesManager.GetProxyHandler(
            serviceProvider,
            request.Outbox.UserId,
            request.MatchAddress,
            request.ExplicitProxyId,
            [.. request.AvailableProxyIds]
        );
        if (handler is null)
            return NetworkRouteResolution.Failure("已配置代理，但没有匹配到可用代理");

        cancellationToken.ThrowIfCancellationRequested();
        var client = await handler.GetProxyClientAsync(serviceProvider, request.MatchAddress);
        if (client is null)
            return NetworkRouteResolution.Failure($"代理 {handler.Id} 当前没有可用端点");

        var kind = handler.IsDynamic ? NetworkRouteKind.DynamicProxy : NetworkRouteKind.StaticProxy;
        var identity = $"{handler.Id}:{client.ProxyHost}:{client.ProxyPort}";
        return NetworkRouteResolution.Success(new NetworkRoute(kind, identity, client));
    }
}
