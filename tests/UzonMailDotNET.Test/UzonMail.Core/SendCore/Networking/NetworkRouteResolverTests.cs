using MailKit.Net.Proxy;
using Microsoft.Extensions.DependencyInjection;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.Settings;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Networking;

/// <summary>
/// 验证直连、Graph 限制和代理解析的网络路由决策。
/// </summary>
[TestClass]
public sealed class NetworkRouteResolverTests
{
    private static readonly NetworkRouteRequest DirectSmtpRequest = new(
        new OutboxKey(1, 2),
        OutboxType.SMTP,
        "sender@test.com",
        0,
        []
    );

    [TestMethod]
    public async Task ResolveAsync_DirectSmtpReturnsSharedDirectRoute()
    {
        var manager = new StubProxiesManager();

        var result = await CreateResolver(manager).ResolveAsync(DirectSmtpRequest);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreSame(NetworkRoute.Direct, result.Route);
        Assert.AreEqual(0, manager.ResolveCount);
    }

    [TestMethod]
    public async Task ResolveAsync_GraphRejectsProxyAndAllowsDirectConnection()
    {
        var resolver = CreateResolver(new StubProxiesManager());
        var proxied = DirectSmtpRequest with { OutboxType = OutboxType.MsGraph, ExplicitProxyId = 9 };
        var direct = DirectSmtpRequest with { OutboxType = OutboxType.MsGraph };

        var failure = await resolver.ResolveAsync(proxied);
        var success = await resolver.ResolveAsync(direct);

        Assert.IsFalse(failure.IsSuccess);
        Assert.AreEqual(SendFailureKind.Proxy, failure.FailureKind);
        StringAssert.Contains(failure.Message, "不支持代理");
        Assert.AreSame(NetworkRoute.Direct, success.Route);
    }

    [TestMethod]
    public async Task ResolveAsync_ConfiguredProxyWithoutHandlerReturnsFailure()
    {
        var result = await CreateResolver(new StubProxiesManager()).ResolveAsync(
            DirectSmtpRequest with { AvailableProxyIds = [7] }
        );

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "没有匹配到");
    }

    [TestMethod]
    public async Task ResolveAsync_HandlerWithoutEndpointReturnsFailure()
    {
        var manager = new StubProxiesManager { Handler = new StubProxyHandler("proxy-1", false) };

        var result = await CreateResolver(manager).ResolveAsync(
            DirectSmtpRequest with { ExplicitProxyId = 7 }
        );

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "proxy-1");
    }

    [TestMethod]
    [DataRow(false, NetworkRouteKind.StaticProxy)]
    [DataRow(true, NetworkRouteKind.DynamicProxy)]
    public async Task ResolveAsync_HealthyHandlerReturnsTypedProxyRoute(
        bool isDynamic,
        NetworkRouteKind expectedKind
    )
    {
        var handler = new StubProxyHandler("proxy-2", isDynamic);
        handler.Client = new ProxyClientAdapter(handler, new Socks5Client("proxy.test", 1080));
        var manager = new StubProxiesManager { Handler = handler };

        var result = await CreateResolver(manager).ResolveAsync(
            DirectSmtpRequest with { ExplicitProxyId = 7 }
        );

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedKind, result.Route!.Kind);
        Assert.AreEqual("proxy-2:proxy.test:1080", result.Route.Identity);
        Assert.AreSame(handler.Client, result.Route.ProxyClient);
    }

    [TestMethod]
    public async Task ResolveAsync_CancelledRequestThrowsBeforeProxyLookup()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            CreateResolver(new StubProxiesManager()).ResolveAsync(
                DirectSmtpRequest,
                cancellation.Token
            )
        );
    }

    private static NetworkRouteResolver CreateResolver(IProxiesManager manager) =>
        new(new ServiceCollection().BuildServiceProvider(), manager);

    private sealed class StubProxiesManager : IProxiesManager
    {
        internal IProxyHandler? Handler { get; init; }

        internal int ResolveCount { get; private set; }

        public Task UpdateUserProxies(IServiceProvider serviceProvider, long userId) =>
            Task.CompletedTask;

        public Task<IProxyHandler?> GetProxyHandler(
            IServiceProvider serviceProvider,
            long userId,
            string outboxEmail,
            long proxyId,
            List<long>? availableProxyIds = null
        )
        {
            ResolveCount++;
            return Task.FromResult(Handler);
        }
    }

    private sealed class StubProxyHandler(string id, bool isDynamic) : IProxyHandler
    {
        internal ProxyClientAdapter? Client { get; set; }

        public string Id { get; } = id;

        public int Priority => 0;

        public bool IsDynamic { get; } = isDynamic;

        public bool IsEnable() => true;

        public void MarkHealthless() { }

        public bool IsMatch(string email) => true;

        public Task<ProxyClientAdapter?> GetProxyClientAsync(
            IServiceProvider serviceProvider,
            string email
        ) => Task.FromResult(Client);

        public void Update(
            Proxy proxy,
            ProxyZoneType proxyZoneType = ProxyZoneType.Default,
            int expireSeconds = int.MaxValue,
            int maxUsedCountPerDomain = -1,
            long userId = 0
        ) { }

        public void DisposeHandler() { }
    }
}
