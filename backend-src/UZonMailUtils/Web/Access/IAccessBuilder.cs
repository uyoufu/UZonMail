using System.Collections.Generic;
using System.Threading.Tasks;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Utils.Web.Access
{
    /// <summary>
    /// 权限码构建器
    /// 只要实现该接口并注册为 service, 程序会自动调用
    /// </summary>
    public interface IAccessBuilder : IScopedService<IAccessBuilder>
    {
        /// <summary>
        /// 生成用户的权限码
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Task<Dictionary<long, List<string>>> GenerateUserPermissionCodes(List<long> userIds);
    }
}
