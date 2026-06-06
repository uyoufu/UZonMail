namespace UZonMail.CorePlugin.SignalRHubs.Permission
{
    public interface IPermissionClient
    {
        /// <summary>
        /// 通知权限更新
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        Task PermissionUpdated(List<string> permissions);
    }
}
