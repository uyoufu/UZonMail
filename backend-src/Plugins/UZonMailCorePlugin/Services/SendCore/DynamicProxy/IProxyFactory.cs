using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.Core.Services.SendCore.DynamicProxy
{
    /// <summary>
    /// Proxy 工厂，通过工厂创建具体的代理
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// 工厂优先级
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public BaseProxy CreateProxy(Proxy proxy);
    }
}
