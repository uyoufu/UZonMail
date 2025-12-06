using UZonMail.CorePlugin.Services.SendCore.Proxies.ProxyTesters;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.SendCore.Proxies.Clients
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
        /// <param name="email"></param>
        /// <returns></returns>
        bool IsMatch(string email);

        /// <summary>
        /// 异步获取代理客户端
        /// </summary>
        /// <param name="email">每个客户端都有一个匹配规则，只有 matchStr 匹配到后，才会返回</param>
        /// <returns></returns>
        Task<ProxyClientAdapter?> GetProxyClientAsync(
            IServiceProvider serviceProvider,
            string email
        );

        /// <summary>
        /// 更新代理操作器
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="proxyZoneType"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="maxUsedCountPerDomain">单个 domain 的最大使用数</param>
        void Update(
            Proxy proxy,
            ProxyZoneType proxyZoneType = ProxyZoneType.Default,
            int expireSeconds = int.MaxValue,
            int maxUsedCountPerDomain = -1,
            long userId = 0
        );
    }
}
