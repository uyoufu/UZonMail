using MailKit.Net.Proxy;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.SendCore.DynamicProxy.Clients
{
    /// <summary>
    /// 代理解析器
    /// </summary>
    public interface IProxyHandler : ITransientService, IProxyHandlerDisposer
    {
        /// <summary>
        /// 代理操作器的 Id, 该 id 应是唯一的
        /// 对于单个代理，该 id 为 Proxy.Id
        /// 对于动态代理，该 id 为 Host
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 是否可用
        /// </summary>
        bool IsEnable();

        /// <summary>
        /// 标记为不健康，只有下一次 ping 通后才会恢复
        /// </summary>
        void MarkHealthless();

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="matchStr"></param>
        /// <param name="limitCount">限制数量</param>
        /// <returns></returns>
        bool IsMatch(string matchStr, int limitCount);

        /// <summary>
        /// 异步获取代理客户端
        /// </summary>
        /// <param name="matchStr">每个客户端都有一个匹配规则，只有 matchStr 匹配到后，才会返回</param>
        /// <returns></returns>
        Task<ProxyClientAdapter?> GetProxyClientAsync(IServiceProvider serviceProvider, string matchStr);

        /// <summary>
        /// 更新代理操作器
        /// </summary>
        /// <param name="proxy"></param>
        void Update(Proxy proxy, int expireSeconds = int.MaxValue);
    }
}
