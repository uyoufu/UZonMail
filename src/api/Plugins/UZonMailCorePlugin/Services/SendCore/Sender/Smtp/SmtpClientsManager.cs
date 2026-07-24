using System.Collections.Concurrent;
using log4net;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.SendCore.Contexts;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Networking;
using UzonMail.CorePlugin.Services.SendCore.Outboxes;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Services.Settings.Model;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Results;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.Sender.Smtp;

/// <summary>
/// 按发件箱、协议配置和网络出口缓存 SMTP 会话。
/// </summary>
public sealed class SmtpClientsManager : ISingletonService, IAsyncDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SmtpClientsManager));
    private readonly ConcurrentDictionary<SmtpClientKey, ThrottlingSmtpClient> _clients = [];
    private readonly AppSettingsManager _settingsService;
    private readonly SmtpConnector _connector;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _maintenanceTask;

    public SmtpClientsManager(AppSettingsManager settingsService, SmtpConnector connector)
    {
        _settingsService = settingsService;
        _connector = connector;
        _maintenanceTask = MaintainConnectionsAsync(_shutdown.Token);
    }

    public async Task<Result<ThrottlingSmtpClient>> GetSmtpClientAsync(
        SendingContext context,
        NetworkRoute route,
        CancellationToken cancellationToken = default
    )
    {
        var outbox = context.EmailItem!.Outbox;
        var key = new SmtpClientKey(
            new OutboxKey(outbox.UserId, outbox.Id),
            GetProfileFingerprint(outbox),
            route.Identity,
            outbox.Email
        );

        if (_clients.TryGetValue(key, out var cached))
        {
            if (await IsAvailableAsync(cached, context, cancellationToken))
                return new Result<ThrottlingSmtpClient> { Data = cached };

            await DisposeSmtpClientAsync(key);
        }

        var client = context.Provider.GetRequiredService<ThrottlingSmtpClient>();
        client.SetParams(key, 0);
        client.ProxyClient = route.ProxyClient;
        try
        {
            var debugConfig = context.Provider.GetRequiredService<DebugConfig>();
            await _connector.ConnectAndAuthenticateAsync(
                client,
                new SmtpConnectionProfile(
                    outbox.SmtpHost,
                    outbox.SmtpPort,
                    outbox.ConnectionSecurity.ToMailKitSecureSocketOptions(),
                    outbox.SmtpAuthUserName ?? outbox.Email,
                    outbox.PlainPassword ?? string.Empty,
                    debugConfig.IsDemo
                ),
                cancellationToken
            );

            if (_clients.TryAdd(key, client))
                return new Result<ThrottlingSmtpClient> { Data = client };

            await DisconnectAndDisposeAsync(client);
            return _clients.TryGetValue(key, out cached)
                ? new Result<ThrottlingSmtpClient> { Data = cached }
                : Result<ThrottlingSmtpClient>.Fail("SMTP 会话并发创建失败");
        }
        catch (Exception exception)
        {
            Logger.Warn(exception);
            await DisconnectAndDisposeAsync(client);
            return Result<ThrottlingSmtpClient>.Fail(exception.Message);
        }
    }

    private async Task<bool> IsAvailableAsync(
        ThrottlingSmtpClient client,
        SendingContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await client.NoOpAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            Logger.Debug($"SMTP 会话 {client.GetClientKey()} 已失效", exception);
            return false;
        }

        if (!client.IsConnected)
            return false;
        if (!client.GetClientKey().HasProxy)
            return true;
        if (client.ProxyClient is not ProxyClientAdapter proxy || !proxy.IsEnable)
            return false;

        var setting = await _settingsService.GetSetting<SendingSetting>(
            context.SqlContext,
            context.EmailItem!.UserId
        );
        return setting.ChangeIpAfterEmailCount <= 0
            || client.SentCount == 0
            || client.SentCount % setting.ChangeIpAfterEmailCount != 0;
    }

    private async Task MaintainConnectionsAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                foreach (var entry in _clients.ToArray())
                {
                    try
                    {
                        await entry.Value.NoOpAsync(cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        Logger.Warn($"SMTP 会话 {entry.Key} 保活失败", exception);
                        await DisposeSmtpClientAsync(entry.Key);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    public ICollection<SmtpClientKey> SmtpClientKeys => _clients.Keys;

    public async Task DisposeSmtpClientAsync(SmtpClientKey key)
    {
        if (_clients.TryRemove(key, out var client))
            await DisconnectAndDisposeAsync(client);
    }

    public async Task DisposeSmtpClientsAsync(OutboxKey outbox)
    {
        foreach (var key in _clients.Keys.Where(x => x.Outbox == outbox).ToList())
            await DisposeSmtpClientAsync(key);
    }

    [Obsolete("Use DisposeSmtpClientAsync instead.")]
    public void DisposeSmtpClient(SmtpClientKey key) => _ = DisposeSmtpClientAsync(key);

    [Obsolete("Use DisposeSmtpClientsAsync instead.")]
    public void DisposeSmtpClients(string email)
    {
        foreach (var key in _clients.Keys.Where(x => x.Email == email).ToList())
            _ = DisposeSmtpClientAsync(key);
    }

    public async ValueTask DisposeAsync()
    {
        await _shutdown.CancelAsync();
        try
        {
            await _maintenanceTask;
        }
        catch (OperationCanceledException) { }

        foreach (var key in _clients.Keys.ToList())
            await DisposeSmtpClientAsync(key);
        _shutdown.Dispose();
    }

    private static async Task DisconnectAndDisposeAsync(ThrottlingSmtpClient client)
    {
        try
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
        catch (Exception exception)
        {
            Logger.Debug("释放 SMTP 会话时断开连接失败", exception);
        }
        finally
        {
            client.Dispose();
        }
    }

    private static string GetProfileFingerprint(OutboxEmailAddress outbox) =>
        $"{outbox.SmtpHost}\n{outbox.SmtpPort}\n{outbox.SmtpAuthUserName}\n{outbox.PlainPassword}\n{outbox.ConnectionSecurity}".MD5();
}
