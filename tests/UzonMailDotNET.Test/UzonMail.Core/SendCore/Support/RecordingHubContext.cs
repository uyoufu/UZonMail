using Microsoft.AspNetCore.SignalR;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.CorePlugin.SignalRHubs.Notify;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.SQL.Core.Emails;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

/// <summary>
/// 记录 SendCore 发出的 SignalR 消息，避免测试依赖真实客户端连接。
/// </summary>
internal sealed class RecordingHubContext : IHubContext<UzonMailHub, IUzonMailClient>
{
    internal RecordingHubContext()
    {
        Client = new RecordingUzonMailClient();
        Clients = new RecordingHubClients(Client);
        Groups = new NoOpGroupManager();
    }

    internal RecordingUzonMailClient Client { get; }

    public IHubClients<IUzonMailClient> Clients { get; }

    public IGroupManager Groups { get; }

    private sealed class RecordingHubClients(IUzonMailClient client) : IHubClients<IUzonMailClient>
    {
        public IUzonMailClient All => client;

        public IUzonMailClient AllExcept(IReadOnlyList<string> excludedConnectionIds) => client;

        public IUzonMailClient Client(string connectionId) => client;

        public IUzonMailClient Clients(IReadOnlyList<string> connectionIds) => client;

        public IUzonMailClient Group(string groupName) => client;

        public IUzonMailClient GroupExcept(
            string groupName,
            IReadOnlyList<string> excludedConnectionIds
        ) => client;

        public IUzonMailClient Groups(IReadOnlyList<string> groupNames) => client;

        public IUzonMailClient User(string userId) => client;

        public IUzonMailClient Users(IReadOnlyList<string> userIds) => client;
    }

    private sealed class NoOpGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task RemoveFromGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;
    }
}

/// <summary>
/// 保存强类型 UzonMail Hub 客户端收到的消息。
/// </summary>
internal sealed class RecordingUzonMailClient : IUzonMailClient
{
    internal List<SendingGroupProgressArg> GroupProgressMessages { get; } = [];

    internal List<SendingItemStatusChangedArg> ItemStatusMessages { get; } = [];

    internal List<string> SendErrors { get; } = [];

    internal List<NotifyMessage> Notifications { get; } = [];

    internal List<List<string>> PermissionUpdates { get; } = [];

    internal List<Outbox> OutboxUpdates { get; } = [];

    internal List<Inbox> InboxUpdates { get; } = [];

    public Task SendingGroupProgressChanged(SendingGroupProgressArg arg)
    {
        GroupProgressMessages.Add(arg);
        return Task.CompletedTask;
    }

    public Task SendingItemStatusChanged(SendingItemStatusChangedArg arg)
    {
        ItemStatusMessages.Add(arg);
        return Task.CompletedTask;
    }

    public Task SendError(string message)
    {
        SendErrors.Add(message);
        return Task.CompletedTask;
    }

    public Task Notify(NotifyMessage message)
    {
        Notifications.Add(message);
        return Task.CompletedTask;
    }

    public Task PermissionUpdated(List<string> permissions)
    {
        PermissionUpdates.Add(permissions);
        return Task.CompletedTask;
    }

    public Task OutboxStatusChanged(Outbox outbox)
    {
        OutboxUpdates.Add(outbox);
        return Task.CompletedTask;
    }

    public Task InboxStatusChanged(Inbox inbox)
    {
        InboxUpdates.Add(inbox);
        return Task.CompletedTask;
    }
}
