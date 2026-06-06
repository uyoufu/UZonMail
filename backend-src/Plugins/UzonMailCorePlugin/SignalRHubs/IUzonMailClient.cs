using UzonMail.CorePlugin.SignalRHubs.Notify;
using UzonMail.CorePlugin.SignalRHubs.OutboxInfo;
using UzonMail.CorePlugin.SignalRHubs.Permission;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;

namespace UzonMail.CorePlugin.SignalRHubs
{
    /// <summary>
    /// 客户端的方法
    /// </summary>
    public interface IUzonMailClient
        : ISendEmailClient,
            INotifyClient,
            IPermissionClient,
            IOutboxStatusChanged { }
}
