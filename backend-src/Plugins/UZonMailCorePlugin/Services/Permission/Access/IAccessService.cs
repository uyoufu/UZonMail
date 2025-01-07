using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.Permission.Access
{
    /// <summary>
    /// 授权服务
    /// 注册为 Scoped 生命周期
    /// </summary>
    public interface IAccessService : IScopedService<IAccessService>
    {
        /// <summary>
        /// 获取用户的权限码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<string>> GetUserPermissionCodes(long userId);
    }
}
