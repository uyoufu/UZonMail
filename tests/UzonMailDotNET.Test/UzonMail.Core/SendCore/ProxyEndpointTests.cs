using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.Proxies.HandlerFactory;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class ProxyEndpointTests
{
    [TestMethod]
    public void StaticProxy_ParsesEscapedCredentials()
    {
        var parsed = ProxyEndpoint.TryCreate(
            "socks5://user%40mail:p%3Ass@127.0.0.1:1080",
            out var endpoint,
            out var error
        );

        Assert.IsTrue(parsed, error);
        Assert.AreEqual("socks5", endpoint!.Scheme);
        Assert.AreEqual("user@mail", endpoint.Username);
        Assert.AreEqual("p:ss", endpoint.Password);
    }

    [TestMethod]
    public void StaticFactory_DoesNotClaimDynamicProviderUrl()
    {
        var factory = new SingleProxyFactory();

        Assert.IsFalse(factory.CanHandle(new Uri("https://provider.example/api/get?count=1")));
        Assert.IsTrue(factory.CanHandle(new Uri("http://127.0.0.1:8080")));
    }
}
