using MailKit.Net.Proxy;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Sender;
using UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;
using UzonMail.CorePlugin.Services.SendCore.Transport;
using UzonMail.DB.SQL.Core.Emails;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Sender;

/// <summary>
/// 验证 SMTP 连接流程、会话缓存、失效替换和资源清理。
/// </summary>
[TestClass]
public sealed class SmtpSessionManagementTests
{
    [TestMethod]
    public async Task Connector_ConnectsAndAuthenticatesUnlessDemoModeSkipsAuthentication()
    {
        var connector = new SmtpConnector();
        var authenticated = new StubSmtpSession();
        var demo = new StubSmtpSession();

        await connector.ConnectAndAuthenticateAsync(
            authenticated,
            new SmtpConnectionProfile("smtp.test", 465, SecureSocketOptions.SslOnConnect, "user", "pass")
        );
        await connector.ConnectAndAuthenticateAsync(
            demo,
            new SmtpConnectionProfile("smtp.test", 25, SecureSocketOptions.None, "user", "pass", true)
        );

        Assert.AreEqual(1, authenticated.ConnectCount);
        Assert.AreEqual(1, authenticated.AuthenticateCount);
        Assert.AreEqual(1, demo.ConnectCount);
        Assert.AreEqual(0, demo.AuthenticateCount);
    }

    [TestMethod]
    public async Task Manager_CreatesCachesAndReusesHealthyDirectSession()
    {
        var session = new StubSmtpSession();
        var factory = new QueueSessionFactory(session);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();

        var created = await manager.GetSmtpClientAsync(context, NetworkRoute.Direct);
        var cached = await manager.GetSmtpClientAsync(context, NetworkRoute.Direct);

        Assert.IsTrue(created.Ok);
        Assert.AreSame(session, created.Data);
        Assert.AreSame(session, cached.Data);
        Assert.AreEqual(1, factory.CreateCount);
        Assert.AreEqual(1, session.ConnectCount);
        Assert.AreEqual(1, session.AuthenticateCount);
        Assert.AreEqual(1, session.NoOpCount);
        Assert.HasCount(1, manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_ReplacesDisconnectedOrUnresponsiveCachedSession()
    {
        var first = new StubSmtpSession();
        var second = new StubSmtpSession();
        var factory = new QueueSessionFactory(first, second);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();
        await manager.GetSmtpClientAsync(context, NetworkRoute.Direct);
        first.NoOpException = new InvalidOperationException("closed");

        var replacement = await manager.GetSmtpClientAsync(context, NetworkRoute.Direct);

        Assert.AreSame(second, replacement.Data);
        Assert.IsTrue(first.Disposed);
        Assert.AreEqual(1, first.DisconnectCount);
        Assert.AreEqual(2, factory.CreateCount);
    }

    [TestMethod]
    public async Task Manager_ConnectionFailureReturnsFailureAndDisposesSession()
    {
        var session = new StubSmtpSession { ConnectException = new IOException("offline") };
        await using var manager = new SmtpClientsManager(
            null!,
            new SmtpConnector(),
            new QueueSessionFactory(session)
        );

        var result = await manager.GetSmtpClientAsync(CreateContext(), NetworkRoute.Direct);

        Assert.IsFalse(result.Ok);
        Assert.AreEqual("offline", result.Message);
        Assert.IsTrue(session.Disposed);
        Assert.IsEmpty(manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_SeparatesRoutesAndDisposesByKeyOrOutbox()
    {
        var direct = new StubSmtpSession();
        var proxied = new StubSmtpSession();
        var factory = new QueueSessionFactory(direct, proxied);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();
        await manager.GetSmtpClientAsync(context, NetworkRoute.Direct);
        var proxyRoute = new NetworkRoute(NetworkRouteKind.StaticProxy, "proxy-1", null);
        await manager.GetSmtpClientAsync(context, proxyRoute);
        Assert.HasCount(2, manager.SmtpClientKeys);

        await manager.DisposeSmtpClientAsync(direct.GetClientKey());
        Assert.IsTrue(direct.Disposed);
        Assert.HasCount(1, manager.SmtpClientKeys);
        await manager.DisposeSmtpClientsAsync(new OutboxKey(30, 20));
        Assert.IsTrue(proxied.Disposed);
        Assert.IsEmpty(manager.SmtpClientKeys);
    }

    [TestMethod]
    public void SmtpKeyAndResultParserExposeStableValues()
    {
        var direct = new SmtpClientKey(new OutboxKey(1, 2), "profile", "direct", "a@test.com");
        var proxy = direct with { RouteIdentity = "proxy" };

        Assert.IsFalse(direct.HasProxy);
        Assert.IsTrue(proxy.HasProxy);
        Assert.AreEqual("receipt", new ResultParser("receipt").GetReceiptId());
    }

    [TestMethod]
    public void EmailSendersManager_RequiresExactlyOneMatchingTransport()
    {
        var smtp = new StubTransport(OutboxType.SMTP);
        Assert.AreSame(smtp, new EmailSendersManager([smtp]).GetEmailSender(OutboxType.SMTP));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new EmailSendersManager([]).GetEmailSender(OutboxType.SMTP)
        );
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new EmailSendersManager([smtp, new StubTransport(OutboxType.SMTP)]).GetEmailSender(
                OutboxType.SMTP
            )
        );
    }

    private static SendingContext CreateContext()
    {
        var configuration = new ConfigurationBuilder().Build();
        var provider = new ServiceCollection()
            .AddSingleton(new DebugConfig(configuration))
            .BuildServiceProvider();
        var prepared = SendCoreTestEntityFactory.CreatePreparedItem();
        var descriptor = new SendItemDescriptor(
            prepared.SourceItem.Id,
            prepared.SourceItem.SendingGroupId,
            prepared.Outbox.Id,
            0
        );
        var lease = new SendLease(
            Guid.CreateVersion7(),
            descriptor,
            new OutboxKey(prepared.UserId, prepared.Outbox.Id),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        return new SendingContext(provider, null!, null!)
        {
            CurrentAttempt = new SendItemExecution(lease, prepared),
        };
    }

    private sealed class QueueSessionFactory(params StubSmtpSession[] sessions) : ISmtpSessionFactory
    {
        private readonly Queue<StubSmtpSession> _sessions = new(sessions);

        internal int CreateCount { get; private set; }

        public ISmtpSession Create(IServiceProvider serviceProvider)
        {
            CreateCount++;
            return _sessions.Dequeue();
        }
    }

    private sealed class StubSmtpSession : ISmtpSession
    {
        private SmtpClientKey _key;

        internal int ConnectCount { get; private set; }
        internal int AuthenticateCount { get; private set; }
        internal int NoOpCount { get; private set; }
        internal int DisconnectCount { get; private set; }
        internal bool Disposed { get; private set; }
        internal Exception? ConnectException { get; init; }
        internal Exception? NoOpException { get; set; }

        public bool IsConnected { get; private set; }
        public IProxyClient? ProxyClient { get; set; }
        public int SentCount { get; private set; }

        public void SetParams(SmtpClientKey clientKey, int cooldownMilliseconds) => _key = clientKey;

        public SmtpClientKey GetClientKey() => _key;

        public Task ConnectAsync(
            string host,
            int port,
            SecureSocketOptions options,
            CancellationToken cancellationToken = default
        )
        {
            ConnectCount++;
            if (ConnectException is not null)
                return Task.FromException(ConnectException);
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task AuthenticateAsync(
            string userName,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            AuthenticateCount++;
            return Task.CompletedTask;
        }

        public Task NoOpAsync(CancellationToken cancellationToken = default)
        {
            NoOpCount++;
            return NoOpException is null ? Task.CompletedTask : Task.FromException(NoOpException);
        }

        public Task<string> SendMessageAsync(
            MimeMessage message,
            CancellationToken cancellationToken = default
        )
        {
            SentCount++;
            return Task.FromResult("receipt");
        }

        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        {
            DisconnectCount++;
            IsConnected = false;
            return Task.CompletedTask;
        }

        public void Dispose() => Disposed = true;
    }

    private sealed class StubTransport(OutboxType type) : IEmailTransport
    {
        public OutboxType Type { get; } = type;

        public Task<TransportResult> SendAsync(
            SendingContext context,
            MimeMessage message,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(TransportResult.Success());

        public Task<TransportResult> ValidateAsync(
            IServiceProvider scopeServiceProvider,
            Outbox outbox,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(TransportResult.Success());
    }
}
