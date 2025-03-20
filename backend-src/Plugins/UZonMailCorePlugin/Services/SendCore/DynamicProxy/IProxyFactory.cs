using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.DynamicProxy
{
    /// <summary>
    /// Proxy 工厂，通过工厂创建具体的代理
    /// 程序会自动调用所有实现了 IProxyFactory 接口的类，创建代理
    /// </summary>
    public interface IProxyFactory : IScopedService<IProxyFactory>
    {
        /// <summary>
        /// 工厂调用优先级
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public IProxyHandler? CreateProxy(Proxy proxy);
    }
}
