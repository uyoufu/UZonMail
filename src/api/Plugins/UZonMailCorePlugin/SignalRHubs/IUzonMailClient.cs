using UzonMail.CorePlugin.SignalRHubs.Notify;
using UzonMail.CorePlugin.SignalRHubs.Permission;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.CorePlugin.SignalRHubs.SenderAccountInfo;

namespace UzonMail.CorePlugin.SignalRHubs
{
    /// <summary>
    /// 客户端的方法
    /// </summary>
    public interface IUzonMailClient
        : ISendEmailClient,
            INotifyClient,
            IPermissionClient,
            ISenderAccountStatusChanged { }
}
