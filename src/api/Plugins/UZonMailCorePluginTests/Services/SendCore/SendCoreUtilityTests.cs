using UzonMail.CorePlugin.Services.Encrypt.Models;
using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Interfaces;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UzonMail.CorePlugin.Services.SendCore.Sender.MsGraph;
using UzonMail.CorePlugin.Services.SendCore.WaitList;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Results;

namespace UzonMail.CorePluginTests.Services.SendCore
{
    [TestClass]
    public class SendCoreUtilityTests
    {
        [TestMethod]
        public void GetSendMailApiPath_ReturnsMe_ForPersonalAccount()
        {
            var result = MsGraphClient.GetSendMailApiPath("user@example.com", true);

            Assert.AreEqual("me", result);
        }

        [TestMethod]
        public void GetSendMailApiPath_ReturnsEncodedUserPath_ForOrganizationAccount()
        {
            var result = MsGraphClient.GetSendMailApiPath("user@example.com", false);

            Assert.AreEqual("users/user%40example.com", result);
        }

        [TestMethod]
        public void GetDataByWeight_DoesNotUseWeightTotalAsIndex()
        {
            var values = new Dictionary<int, TestWeight>
            {
                [1] = new(1, true),
                [2] = new(100, true)
            };

            for (var i = 0; i < 100; i++)
            {
                var result = values.GetDataByWeight<int, TestWeight>();
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void MatchSendingMeta_IncludesSpecificItemsInRecycleBin()
        {
            var metas = new SendingItemMetaList();
            metas.Add(new SendItemMeta(1, 7));

            var meta = metas.GetSendingMeta(7);

            Assert.IsNotNull(meta);
            Assert.IsTrue(metas.MatchSendingMeta(7, true));
        }

        [TestMethod]
        public void SendingGroupStatusMapper_MapsCancelToCancel()
        {
            var result = SendingGroupStatusMapper.ToSendingItemStatus(SendingGroupStatus.Cancel);

            Assert.AreEqual(SendingItemStatus.Cancel, result);
        }

        [TestMethod]
        public void SendingGroupStatusMapper_MapsPauseToFailed()
        {
            var result = SendingGroupStatusMapper.ToSendingItemStatus(SendingGroupStatus.Pause);

            Assert.AreEqual(SendingItemStatus.Failed, result);
        }

        [TestMethod]
        public void OutboxEmailAddress_GetSendingGroupIds_ReturnsDistinctGroupIds()
        {
            var outbox = CreateOutboxEmailAddress(
                OutboxEmailAddressType.Specific,
                sendingItemIds: [10, 11]
            );

            var groupIds = outbox.GetSendingGroupIds();

            CollectionAssert.AreEqual(new List<long> { 1 }, groupIds);
        }

        [TestMethod]
        public void OutboxEmailAddress_Update_MergesSharedAndSpecificTargets()
        {
            var specific = CreateOutboxEmailAddress(
                OutboxEmailAddressType.Specific,
                sendingItemIds: [10]
            );
            var shared = CreateOutboxEmailAddress(OutboxEmailAddressType.Shared);

            specific.Update(shared);
            specific.RemoveSepecificSendingItem(1, 10);

            Assert.IsTrue(specific.ContainsSendingGroup(1));
            Assert.AreEqual(OutboxEmailAddressType.Specific | OutboxEmailAddressType.Shared, specific.Type);
        }

        [TestMethod]
        public void ProxyEndpoint_CanParseSupportedProxySchemes()
        {
            var parsed = ProxyEndpoint.TryCreate(
                "socks4a://user:pass@127.0.0.1:1080",
                out var endpoint,
                out var errorMessage
            );

            Assert.IsTrue(parsed, errorMessage);
            Assert.IsNotNull(endpoint);
            Assert.AreEqual("socks4a", endpoint.Scheme);
            Assert.AreEqual("127.0.0.1", endpoint.Host);
            Assert.AreEqual(1080, endpoint.Port);
            Assert.AreEqual("user", endpoint.Username);
            Assert.AreEqual("pass", endpoint.Password);
        }

        [TestMethod]
        public void ProxyEndpoint_RejectsDynamicApiUrls()
        {
            var parsed = ProxyEndpoint.TryCreate(
                "http://api.proxy.ipidea.io/getBalanceProxyIp?num=100&return_type=txt",
                out _,
                out _
            );

            Assert.IsFalse(parsed);
        }

        [TestMethod]
        public async Task ProxyHandler_GetProxyClientAsync_CreatesSingleAdapterConcurrently()
        {
            var handler = CreateHealthyProxyHandler("socks5://user:pass@127.0.0.1:1080");
            await handler.HealthCheck();

            var clients = await Task.WhenAll(
                Enumerable
                    .Range(0, 20)
                    .Select(_ => handler.GetProxyClientAsync(new EmptyServiceProvider(), "sender@example.com"))
            );

            Assert.IsTrue(clients.All(x => x != null));
            Assert.IsTrue(clients.All(x => ReferenceEquals(clients[0], x)));
        }

        [TestMethod]
        public async Task ProxyHandler_Update_RebuildsAdapterWhenEndpointChanges()
        {
            var handler = CreateHealthyProxyHandler("socks5://127.0.0.1:1080");
            await handler.HealthCheck();
            var firstClient = await handler.GetProxyClientAsync(
                new EmptyServiceProvider(),
                "sender@example.com"
            );

            handler.Update(CreateProxy("socks5://127.0.0.2:1080"));
            await handler.HealthCheck();
            var secondClient = await handler.GetProxyClientAsync(
                new EmptyServiceProvider(),
                "sender@example.com"
            );

            Assert.IsNotNull(firstClient);
            Assert.IsNotNull(secondClient);
            Assert.AreNotSame(firstClient, secondClient);
            Assert.AreEqual("127.0.0.2", secondClient.ProxyHost);
        }

        [TestMethod]
        public async Task ProxyHandler_HealthCheck_DoesNotRunConcurrently()
        {
            var checker = new TestProxyHealthChecker(delayMs: 100);
            var handler = new ProxyHandler([checker]);
            handler.Update(CreateProxy("socks5://127.0.0.1:1080"));

            await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => handler.HealthCheck()));

            Assert.AreEqual(1, checker.CallCount);
        }

        private static OutboxEmailAddress CreateOutboxEmailAddress(
            OutboxEmailAddressType type,
            List<long>? sendingItemIds = null
        )
        {
            var encryptParams = new EncryptParams();
            return new OutboxEmailAddress(
                new Outbox()
                {
                    Id = 7,
                    UserId = 3,
                    Email = "sender@example.com",
                    Name = "Sender",
                    Password = "password".AES(encryptParams.Key, encryptParams.Iv),
                    ReplyToEmails = "",
                    SmtpHost = "smtp.example.com",
                SmtpPort = 465,
                Weight = 1
            },
                1,
                encryptParams,
                type,
                // 保留 null 语义，避免共享发件箱被当成空的特定发件项列表。
                sendingItemIds!
            );
        }

        private static ProxyHandler CreateHealthyProxyHandler(string url)
        {
            var handler = new ProxyHandler([new TestProxyHealthChecker()]);
            handler.Update(CreateProxy(url));
            return handler;
        }

        private static Proxy CreateProxy(string url)
        {
            return new Proxy()
            {
                Id = 1,
                ObjectId = "proxy-object-id",
                Url = url,
                IsActive = true,
                Name = "test proxy",
            };
        }

        private sealed class TestWeight(int weight, bool enable) : IWeight
        {
            public int Weight => weight;

            public bool Enable => enable;
        }

        private sealed class TestProxyHealthChecker(int delayMs = 0) : IProxyHealthChecker
        {
            private int _callCount;

            public bool Enable => true;

            public int Order => 0;

            public ProxyZoneType ProxyZoneType => ProxyZoneType.Default;

            public int CallCount => _callCount;

            public async Task<Result<string?>> GetIP(string proxyUrl)
            {
                Interlocked.Increment(ref _callCount);
                if (delayMs > 0)
                    await Task.Delay(delayMs);

                return Result<string?>.Success("127.0.0.1");
            }
        }

        private sealed class EmptyServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }
}
