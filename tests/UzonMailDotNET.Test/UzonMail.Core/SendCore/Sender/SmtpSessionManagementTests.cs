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
/// Verifies SMTP connection flow, session leasing, retirement, and resource cleanup.
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
            new SmtpConnectionProfile(
                "smtp.test",
                465,
                SecureSocketOptions.SslOnConnect,
                "user",
                "pass"
            )
        );
        await connector.ConnectAndAuthenticateAsync(
            demo,
            new SmtpConnectionProfile(
                "smtp.test",
                25,
                SecureSocketOptions.None,
                "user",
                "pass",
                true
            )
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

        var created = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        var cached = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var createdLease = created.Data!;
        await using var cachedLease = cached.Data!;

        Assert.IsTrue(created.Ok);
        Assert.IsTrue(cached.Ok);
        Assert.AreEqual(createdLease.ClientKey, cachedLease.ClientKey);
        Assert.AreEqual(1, factory.CreateCount);
        Assert.AreEqual(1, session.ConnectCount);
        Assert.AreEqual(1, session.AuthenticateCount);
        Assert.AreEqual(0, session.NoOpCount);
        Assert.HasCount(1, manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_ReplacesInvalidatedCachedSession()
    {
        var first = new StubSmtpSession();
        var second = new StubSmtpSession();
        var factory = new QueueSessionFactory(first, second);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();

        var initial = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using (var initialLease = initial.Data!)
        {
            initialLease.Invalidate();
        }

        var replacement = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var replacementLease = replacement.Data!;

        Assert.AreEqual(2, factory.CreateCount);
        Assert.IsTrue(first.Disposed);
        Assert.AreEqual(1, first.DisconnectCount);
        Assert.IsFalse(second.Disposed);
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

        var result = await manager.AcquireSmtpSessionAsync(CreateContext(), NetworkRoute.Direct);

        Assert.IsFalse(result.Ok);
        Assert.AreEqual("offline", result.Message);
        Assert.IsTrue(session.Disposed);
        Assert.IsEmpty(manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_CanceledAcquisitionPublishesCompletedConnectionForReuse()
    {
        var connectGate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var session = new StubSmtpSession { ConnectGate = connectGate };
        var factory = new QueueSessionFactory(session);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        using var cancellation = new CancellationTokenSource();

        var canceledAcquisition = manager.AcquireSmtpSessionAsync(
            CreateContext(),
            NetworkRoute.Direct,
            cancellation.Token
        );
        await session.ConnectStarted.Task;
        cancellation.Cancel();

        try
        {
            await canceledAcquisition;
            Assert.Fail("The canceled acquisition should not receive a session lease.");
        }
        catch (OperationCanceledException) { }

        connectGate.SetResult(true);
        var reused = await manager.AcquireSmtpSessionAsync(CreateContext(), NetworkRoute.Direct);
        await using var reusedLease = reused.Data!;

        Assert.IsTrue(reused.Ok);
        Assert.AreEqual(1, factory.CreateCount);
        Assert.HasCount(1, manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_SeparatesRoutesAndDisposesByKeyOrOutbox()
    {
        var direct = new StubSmtpSession();
        var proxied = new StubSmtpSession();
        var factory = new QueueSessionFactory(direct, proxied);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();
        var directResult = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        var proxyRoute = new NetworkRoute(NetworkRouteKind.StaticProxy, "proxy-1", null);
        var proxyResult = await manager.AcquireSmtpSessionAsync(context, proxyRoute);
        await using var directLease = directResult.Data!;
        await using var proxyLease = proxyResult.Data!;

        Assert.HasCount(2, manager.SmtpClientKeys);

        await directLease.DisposeAsync();
        await manager.DisposeSmtpClientAsync(directLease.ClientKey);
        Assert.IsTrue(direct.Disposed);
        Assert.HasCount(1, manager.SmtpClientKeys);

        await proxyLease.DisposeAsync();
        await manager.DisposeSmtpClientsAsync(new OutboxKey(30, 20));
        Assert.IsTrue(proxied.Disposed);
        Assert.IsEmpty(manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_RetirementWaitsForInFlightLease()
    {
        var sendGate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var session = new StubSmtpSession { SendGate = sendGate };
        await using var manager = new SmtpClientsManager(
            null!,
            new SmtpConnector(),
            new QueueSessionFactory(session)
        );
        var result = await manager.AcquireSmtpSessionAsync(CreateContext(), NetworkRoute.Direct);
        await using var lease = result.Data!;

        var sending = lease.SendMessageAsync(new MimeMessage());
        await session.SendStarted.Task;
        lease.Invalidate();

        Assert.IsFalse(session.Disposed);
        sendGate.SetResult(true);
        await sending;
        await lease.DisposeAsync();

        Assert.IsTrue(session.Disposed);
        Assert.AreEqual(1, session.DisconnectCount);
    }

    [TestMethod]
    public async Task Manager_DoesNotRetireReplacementForStaleLease()
    {
        var first = new StubSmtpSession();
        var second = new StubSmtpSession();
        var factory = new QueueSessionFactory(first, second);
        await using var manager = new SmtpClientsManager(null!, new SmtpConnector(), factory);
        var context = CreateContext();
        var firstResult = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var firstLease = firstResult.Data!;

        firstLease.Invalidate();
        var replacementResult = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var replacementLease = replacementResult.Data!;

        firstLease.Invalidate();
        var reusedReplacement = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var reusedReplacementLease = reusedReplacement.Data!;

        Assert.AreEqual(2, factory.CreateCount);
        Assert.IsFalse(second.Disposed);
        Assert.HasCount(1, manager.SmtpClientKeys);
    }

    [TestMethod]
    public async Task Manager_SerializesConcurrentSendsForOneSession()
    {
        var sendGate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var session = new StubSmtpSession { SendGate = sendGate };
        await using var manager = new SmtpClientsManager(
            null!,
            new SmtpConnector(),
            new QueueSessionFactory(session)
        );
        var context = CreateContext();
        var firstResult = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        var secondResult = await manager.AcquireSmtpSessionAsync(context, NetworkRoute.Direct);
        await using var firstLease = firstResult.Data!;
        await using var secondLease = secondResult.Data!;

        var firstSend = firstLease.SendMessageAsync(new MimeMessage());
        await session.SendStarted.Task;
        var secondSend = secondLease.SendMessageAsync(new MimeMessage());
        await Task.Delay(50);

        Assert.AreEqual(1, session.SendCount);
        Assert.AreEqual(1, session.MaximumConcurrentSendCount);

        sendGate.SetResult(true);
        await Task.WhenAll(firstSend, secondSend);
        Assert.AreEqual(1, session.MaximumConcurrentSendCount);
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
        Assert.ThrowsExactly<InvalidOperationException>(
            () => new EmailSendersManager([]).GetEmailSender(OutboxType.SMTP)
        );
        Assert.ThrowsExactly<InvalidOperationException>(
            () =>
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

    private sealed class QueueSessionFactory(params StubSmtpSession[] sessions)
        : ISmtpSessionFactory
    {
        private readonly Queue<StubSmtpSession> _sessions = new(sessions);

        internal int CreateCount { get; private set; }

        public ISmtpSession Create()
        {
            CreateCount++;
            return _sessions.Dequeue();
        }
    }

    private sealed class StubSmtpSession : ISmtpSession
    {
        private int _currentConcurrentSendCount;

        internal int ConnectCount { get; private set; }
        internal int AuthenticateCount { get; private set; }
        internal int NoOpCount { get; private set; }
        internal int DisconnectCount { get; private set; }
        internal int SendCount { get; private set; }
        internal int MaximumConcurrentSendCount { get; private set; }
        internal bool Disposed { get; private set; }
        internal Exception? ConnectException { get; init; }
        internal TaskCompletionSource<bool>? ConnectGate { get; init; }
        internal TaskCompletionSource<bool>? SendGate { get; init; }
        internal TaskCompletionSource<bool> ConnectStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> SendStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsConnected { get; private set; }
        public IProxyClient? ProxyClient { get; set; }
        public int SentCount { get; private set; }

        public void SetParams(SmtpClientKey clientKey, int cooldownMilliseconds) { }

        public async Task ConnectAsync(
            string host,
            int port,
            SecureSocketOptions options,
            CancellationToken cancellationToken = default
        )
        {
            ConnectCount++;
            ConnectStarted.TrySetResult(true);
            if (ConnectException is not null)
                throw ConnectException;
            if (ConnectGate is not null)
                await ConnectGate.Task.WaitAsync(cancellationToken);
            IsConnected = true;
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
            return Task.CompletedTask;
        }

        public async Task<string> SendMessageAsync(
            MimeMessage message,
            CancellationToken cancellationToken = default
        )
        {
            SendCount++;
            var concurrentSendCount = Interlocked.Increment(ref _currentConcurrentSendCount);
            MaximumConcurrentSendCount = Math.Max(MaximumConcurrentSendCount, concurrentSendCount);
            SendStarted.TrySetResult(true);
            try
            {
                if (SendGate is not null)
                    await SendGate.Task.WaitAsync(cancellationToken);

                SentCount++;
                return "receipt";
            }
            finally
            {
                Interlocked.Decrement(ref _currentConcurrentSendCount);
            }
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
