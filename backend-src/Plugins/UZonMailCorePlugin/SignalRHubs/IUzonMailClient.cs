using UZonMail.CorePlugin.SignalRHubs.Notify;
using UZonMail.CorePlugin.SignalRHubs.OutboxInfo;
using UZonMail.CorePlugin.SignalRHubs.Permission;
using UZonMail.CorePlugin.SignalRHubs.SendEmail;

namespace UZonMail.CorePlugin.SignalRHubs
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
