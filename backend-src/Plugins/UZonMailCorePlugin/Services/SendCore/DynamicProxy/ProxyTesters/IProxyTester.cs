using System.Threading.Tasks;
using UZonMail.Utils.Results;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters
{
    /// <summary>
    /// IP 查询接口的基类
    /// </summary>
    public interface IProxyTester : ISingletonService<IProxyTester>
    {
        /// <summary>
        /// 是否可用
        /// </summary>
        bool Enable { get; }

        /// <summary>
        /// 排序号
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 类型
        /// </summary>
        ProxyTesterType TesterType { get; }

        /// <summary>
        /// 获取 IP
        /// 当为 false 时，表示本身不可用
        /// </summary>
        /// <returns></returns>
        Task<Result<string?>> GetIP(string proxyUrl);
    }
}
