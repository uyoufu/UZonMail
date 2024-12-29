using UZonMail.Core.SignalRHubs.Notify;
using UZonMail.Core.SignalRHubs.Permission;
using UZonMail.Core.SignalRHubs.SendEmail;

namespace UZonMail.Core.SignalRHubs
{
    /// <summary>
    /// 客户端的方法
    /// </summary>
    public interface IUzonMailClient: ISendEmailClient, INotifyClient,IPermissionClient
    {
    }
}
