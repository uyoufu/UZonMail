using Microsoft.Extensions.DependencyInjection;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Results;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Proxies;

/// <summary>
/// 验证单代理端点的配置、健康状态、匹配限额和资源释放。
/// </summary>
[TestClass]
public sealed class ProxyHandlerTests
{
    [TestMethod]
    public async Task UnconfiguredHandler_IsDisabledAndCannotCreateClient()
    {
        var handler = new ProxyHandler([]);

        Assert.AreEqual(string.Empty, handler.Id);
        Assert.AreEqual(string.Empty, handler.ToString());
        Assert.IsFalse(handler.IsEnable());
        Assert.IsFalse(handler.IsMatch("sender@test.com"));
        Assert.IsFalse(handler.ShouldHealthCheck);
        Assert.IsFalse(await handler.HealthCheck());
        Assert.IsNull(
            await handler.GetProxyClientAsync(
                new ServiceCollection().BuildServiceProvider(),
                "sender@test.com"
            )
        );
    }

    [TestMethod]
    public async Task HealthyProxy_ExposesConnectionAndEnforcesPerDomainUsageLimit()
    {
        var checker = new StubHealthChecker(Result<string?>.Success("127.0.0.1"));
        var handler = new ProxyHandler([checker]);
        handler.Update(
            CreateProxy(7, "socks5://user:pass@proxy.test:1080"),
            maxUsedCountPerDomain: 1,
            userId: 42
        );

        Assert.AreEqual("7", handler.Id);
        Assert.AreEqual(42L, handler.UserId);
        Assert.AreEqual(0, handler.Priority);
        Assert.AreEqual("socks5", handler.Schema);
        Assert.AreEqual("proxy.test", handler.Host);
        Assert.AreEqual(1080, handler.Port);
        Assert.AreEqual("user", handler.Username);
        Assert.AreEqual("pass", handler.Password);
        Assert.AreEqual("socks5://user:pass@proxy.test:1080", handler.ToString());
        Assert.IsTrue(handler.ShouldHealthCheck);
        Assert.IsTrue(await handler.HealthCheck());
        Assert.IsTrue(handler.IsEnable());

        var provider = new ServiceCollection().BuildServiceProvider();
        var first = await handler.GetProxyClientAsync(provider, "one@test.com");

        Assert.IsNotNull(first);
        Assert.IsFalse(handler.IsMatch("two@test.com"));
        Assert.IsTrue(handler.IsMatch("two@other.com"));
        var cached = await handler.GetProxyClientAsync(provider, "two@other.com");
        Assert.AreSame(first, cached);
        Assert.IsTrue(first.IsEnable);
        first.MarkHealthless();
        Assert.IsFalse(handler.IsEnable());
    }

    [TestMethod]
    public async Task HealthCheck_UsesEnabledMatchingCheckersInOrder()
    {
        var disabled = new StubHealthChecker(Result<string?>.Success("ignored"), enable: false, order: 0);
        var failed = new StubHealthChecker(Result<string?>.Fail("offline"), order: 1);
        var success = new StubHealthChecker(Result<string?>.Success("127.0.0.1"), order: 2);
        var handler = new ProxyHandler([success, disabled, failed]);
        handler.Update(CreateProxy(7, "http://proxy.test:8080"));

        Assert.IsTrue(await handler.HealthCheck());
        Assert.AreEqual(0, disabled.CallCount);
        Assert.AreEqual(1, failed.CallCount);
        Assert.AreEqual(1, success.CallCount);
        Assert.AreEqual("http://proxy.test:8080", handler.ToString());
    }

    [TestMethod]
    public async Task FailedAndThrowingHealthChecksLeaveProxyDisabled()
    {
        var failed = new ProxyHandler([new StubHealthChecker(Result<string?>.Fail("offline"))]);
        failed.Update(CreateProxy(7, "http://proxy.test:8080"));
        var throwing = new ProxyHandler([new StubHealthChecker(new InvalidOperationException("boom"))]);
        throwing.Update(CreateProxy(8, "http://proxy.test:8081"));

        Assert.IsFalse(await failed.HealthCheck());
        Assert.IsFalse(await throwing.HealthCheck());
        Assert.IsFalse(failed.IsEnable());
        Assert.IsFalse(throwing.IsEnable());
    }

    [TestMethod]
    public async Task InvalidUpdate_ClearsPreviouslyValidEndpoint()
    {
        var handler = new ProxyHandler([new StubHealthChecker(Result<string?>.Success("127.0.0.1"))]);
        handler.Update(CreateProxy(7, "http://proxy.test:8080"));
        Assert.IsTrue(await handler.HealthCheck());
        Assert.IsNotNull(
            await handler.GetProxyClientAsync(
                new ServiceCollection().BuildServiceProvider(),
                "sender@test.com"
            )
        );

        handler.Update(CreateProxy(7, "not-a-proxy"));

        Assert.IsFalse(await handler.HealthCheck());
        Assert.AreEqual(string.Empty, handler.ToString());
        Assert.IsNull(
            await handler.GetProxyClientAsync(
                new ServiceCollection().BuildServiceProvider(),
                "sender@test.com"
            )
        );
    }

    [TestMethod]
    public async Task InactiveExpiredRegexAndDisposedStatesAreRejected()
    {
        var handler = new ProxyHandler([new StubHealthChecker(Result<string?>.Success("127.0.0.1"))]);
        var proxy = CreateProxy(7, "http://proxy.test:8080");
        proxy.MatchRegex = "[";
        handler.Update(proxy, expireSeconds: -1);

        Assert.IsTrue(handler.IsExpired);
        Assert.IsFalse(handler.IsMatch("sender@test.com"));
        Assert.IsFalse(await handler.HealthCheck());
        handler.CleanupExpiredResources();
        handler.DisposeHandler();
        handler.DisposeHandler();
        Assert.IsFalse(handler.IsEnable());
        Assert.IsFalse(handler.ShouldHealthCheck);
    }

    private static Proxy CreateProxy(long id, string url) =>
        new() { Id = id, Url = url, IsActive = true };

    private sealed class StubHealthChecker : IProxyHealthChecker
    {
        private readonly Result<string?>? _result;
        private readonly Exception? _exception;

        internal StubHealthChecker(
            Result<string?> result,
            bool enable = true,
            int order = 0
        )
        {
            _result = result;
            Enable = enable;
            Order = order;
        }

        internal StubHealthChecker(Exception exception)
        {
            _exception = exception;
            Enable = true;
        }

        internal int CallCount { get; private set; }

        public bool Enable { get; }

        public int Order { get; }

        public ProxyZoneType ProxyZoneType => ProxyZoneType.Default;

        public Task<Result<string?>> GetIP(string proxyUrl)
        {
            CallCount++;
            return _exception is null
                ? Task.FromResult(_result!)
                : Task.FromException<Result<string?>>(_exception);
        }
    }
}
